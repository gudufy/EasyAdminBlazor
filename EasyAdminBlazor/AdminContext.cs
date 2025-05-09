using EasyAdminBlazor.Infrastructure.Encrypt;
using BootstrapBlazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using System.Reflection;
using Yitter.IdGenerator;

public class AdminContext
{
    public IServiceProvider Service { get; private set; }
    public HttpContext HttpContext { get; }
    private NavigationManager Nav { get; }
    private IJSRuntime JS { get; }
    private IFreeSql _orm;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// 构造函数，初始化 AdminContext 实例
    /// </summary>
    /// <param name="orm">FreeSql 实例</param>
    /// <param name="httpContextAccessor">HttpContext 访问器</param>
    /// <param name="nav">导航管理器</param>
    /// <param name="js">JS 运行时</param>
    /// <param name="memoryCache">缓存</param>
    public AdminContext(IFreeSql orm, IHttpContextAccessor httpContextAccessor, NavigationManager nav, IJSRuntime js, IMemoryCache memoryCache)
    {
        _orm = orm;
        HttpContext = httpContextAccessor.HttpContext;
        Service = HttpContext?.RequestServices;
        Nav = nav;
        JS = js;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// 获取 FreeSql 实例
    /// </summary>
    public IFreeSql Orm => _orm;

    /// <summary>
    /// 检查用户是否已登录
    /// </summary>
    /// <returns>用户已登录返回 true，否则返回 false</returns>
    public bool IsLogin()
    {
        // 检查 HttpContext 及其请求的 Cookies 是否为空
        if (HttpContext?.Request?.Cookies == null || !HttpContext.Request.Cookies.TryGetValue("login", out var cookie))
        {
            return false;
        }

        // 检查 Cookie 是否为空，并尝试解析 Cookie
        return !string.IsNullOrEmpty(cookie) && TryParseCookie(cookie, out var userId, out _) && userId > 0;
    }

    /// <summary>
    /// 初始化用户信息
    /// </summary>
    /// <returns>异步任务</returns>
    async public Task Init()
    {
        // 检查 HttpContext 及其请求的 Cookies 是否为空
        if (HttpContext?.Request?.Cookies == null || !HttpContext.Request.Cookies.TryGetValue("login", out var cookie))
        {
            return;
        }

        // 尝试解析 Cookie
        if (TryParseCookie(cookie, out var userId, out var loginTime) && userId > 0)
        {
            // 根据用户 ID 查询用户信息
            User = await Orm.Select<SysUser>().Where(a => a.Id == userId).FirstAsync();
        }
        else
        {
            // 解析失败，执行退出登录操作
            await SignOut();
            return;
        }

        // 检查用户登录时间是否一致，不一致则处理其他端登录情况
        if (User != null && Math.Abs((User.LoginTime - loginTime).TotalSeconds) > 1)
        {
            await HandleOtherLogin();
        }
    }

    /// <summary>
    /// 处理其他端登录的情况
    /// </summary>
    /// <returns>异步任务</returns>
    private async Task HandleOtherLogin()
    {
        // 将用户信息置空
        User = null;
        // 执行退出登录操作
        await SignOut();
        // 重定向到登录页面
        Redirect($"/Login?Redirect={Uri.EscapeDataString(Nav.Uri)}");
    }

    internal string RandToken = Guid.NewGuid().ToString("n");

    /// <summary>
    /// 用户登录操作
    /// </summary>
    /// <param name="user">用户信息</param>
    /// <param name="remember">是否记住登录状态</param>
    /// <returns>异步任务</returns>
    async public Task SignIn(SysUser user, bool remember)
    {
        _memoryCache.Remove($"UserRoles_{user.Id}");
        _memoryCache.Remove($"UserRoleMenus_{user.Id}");

        // 更新用户登录时间
        user.LoginTime = DateTime.Now;
        await Orm.Update<SysUser>()
            .Where(a => a.Id == user.Id)
            .Set(a => a.LoginTime, user.LoginTime)
            .ExecuteAffrowsAsync();

        // 加密用户 ID 和登录时间
        var encryptedData = DesEncrypt.Encrypt($"{user.Id}|{user.LoginTime:yyyy-MM-dd HH:mm:ss}");
        // 设置登录 Cookie
        await SetLoginCookie(encryptedData, remember);
    }

    /// <summary>
    /// 用户退出登录操作
    /// </summary>
    /// <returns>异步任务</returns>
    async public Task SignOut()
    {
        // 清除登录 Cookie
        await SetLoginCookie(string.Empty, false);
    }

    /// <summary>
    /// 设置登录 Cookie
    /// </summary>
    /// <param name="value">Cookie 值</param>
    /// <param name="remember">是否记住登录状态</param>
    /// <returns>异步任务</returns>
    private async Task SetLoginCookie(string value, bool remember)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // WebSocket 请求，调用 JS 方法设置 Cookie
            await JS.InvokeVoidAsync("EasyAdminBlazorJS.setCookie", "login", value, remember ? 15 : -1);
        }
        else
        {
            // 普通请求，使用 HttpContext 设置 Cookie
            HttpContext.Response.Cookies.Append("login", value, new CookieOptions
            {
                Path = "/",
                Expires = remember ? DateTimeOffset.UtcNow.AddDays(15) : null
            });
        }
    }

    /// <summary>
    /// 尝试解析登录 Cookie
    /// </summary>
    /// <param name="cookie">Cookie 值</param>
    /// <param name="userId">解析出的用户 ID</param>
    /// <param name="loginTime">解析出的登录时间</param>
    /// <returns>解析成功返回 true，否则返回 false</returns>
    static bool TryParseCookie(string cookie, out long userId, out DateTime loginTime)
    {
        userId = 0;
        loginTime = DateTime.MinValue;

        // 检查 Cookie 是否为空
        if (string.IsNullOrEmpty(cookie))
        {
            return false;
        }

        // 解密 Cookie 并分割信息
        var decrypted = DesEncrypt.Decrypt(cookie);
        var info = decrypted.Split('|');

        // 检查分割后的信息长度是否符合要求，并尝试转换用户 ID 和登录时间
        if (info.Length == 2 && long.TryParse(info[0], out userId) && DateTime.TryParse(info[1], out loginTime))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 重定向到指定 URL
    /// </summary>
    /// <param name="url">目标 URL</param>
    public void Redirect(string url)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // WebSocket 请求，使用导航管理器进行导航
            Nav.NavigateTo(url, true);
        }
        else
        {
            // 普通请求，使用 HttpContext 进行重定向
            HttpContext.Response.Redirect(url);
        }
    }

    /// <summary>
    /// 重定向到登录页面
    /// </summary>
    public void RedirectLogin()
    {
        var redirectUrl = HttpContext.WebSockets.IsWebSocketRequest
            ? $"/Admin/Login?Redirect={Uri.EscapeDataString(Nav.Uri)}"
            : $"/Admin/Login?Redirect={Uri.EscapeDataString(HttpContext.Request.GetEncodedPathAndQuery())}";
        // 调用重定向方法
        Redirect(redirectUrl);
    }

    /// <summary>
    /// 修改个人资料
    /// </summary>
    /// <param name="user"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<bool> UpdateProfile(SysUser user, UserProfile model)
    {
        return await Orm.Update<SysUser>().Where(a => a.Id == user.Id).SetDto(model).ExecuteAffrowsAsync() > 0;
    }

    /// <summary>
    /// 修改当前登录用户密码
    /// </summary>
    /// <param name="user">用户信息</param>
    /// <param name="model">密码修改模型</param>
    /// <returns>修改成功返回 true，否则返回 false</returns>
    public async Task<bool> UpdatePassword(SysUser user, UserPassword model)
    {
        var entity = await Orm.Select<SysUser>().Where(a => a.Id == user.Id).FirstAsync();
        // 检查旧密码是否正确
        if (entity != null)
        {
            if(!PBKDF2Encrypt.VerifyPassword(model.OldPassword, entity.Password))
            {
                return false;
            }

            // 加密新密码并更新用户密码
            user.Password = PBKDF2Encrypt.HashPassword(model.NewPassword);
            return await Orm.Update<SysUser>()
                .Where(a => a.Id == user.Id)
                .Set(a => a.Password, user.Password)
                .ExecuteAffrowsAsync() > 0;
        }

        return false;
    }

    public SysUser User { get; private set; }
    public List<SysRole> Roles { get; private set; } = new();
    public List<SysMenu> RoleMenus { get; private set; } = new();

    /// <summary>
    /// 初始化用户角色和菜单信息
    /// </summary>
    /// <returns>异步任务</returns>
    public async Task InitRoles()
    {
        // 若用户为空，直接返回
        if (User == null) return;

        // 生成包含用户 ID 的缓存键
        var rolesCacheKey = $"UserRoles_{User.Id}";
        var roleMenusCacheKey = $"UserRoleMenus_{User.Id}";

        // 尝试从缓存获取 Roles
        if (!_memoryCache.TryGetValue(rolesCacheKey, out List<SysRole>? cachedRoles))
        {
            // 查询用户的角色信息
            cachedRoles = await Orm.Select<SysRole>()
                .Where(a => Orm.Select<SysRoleUser>()
                    .Any(b => b.UserId == User.Id && b.RoleId == a.Id))
                .ToListAsync();

            // 将 Roles 存入缓存，1 分钟后失效
            _memoryCache.Set(rolesCacheKey, cachedRoles, TimeSpan.FromMinutes(1));
        }
        Roles = cachedRoles ?? new();

        // 尝试从缓存获取 RoleMenus
        if (!_memoryCache.TryGetValue(roleMenusCacheKey, out List<SysMenu>? cachedRoleMenus))
        {
            // 检查用户是否为管理员
            if (Roles.Any(a => a.IsAdministrator))
            {
                // 若用户是管理员，获取所有菜单
                cachedRoleMenus = await Orm.Select<SysMenu>().ToListAsync();
            }
            else
            {
                // 获取角色 ID 列表
                var roleIds = Roles.Select(a => a.Id).ToList();

                // 定义查询角色关联菜单的条件
                var menuQuery = Orm.Select<SysMenu>()
                    .Where(a => Orm.Select<SysRoleMenu>()
                        .Any(b => roleIds.Contains(b.RoleId) && b.MenuId == a.Id));

                // 查询角色关联的菜单
                var roleMenus = await menuQuery.ToListAsync();

                // 获取父菜单 ID 列表
                var parentIds = roleMenus.Select(a => a.ParentId).Distinct().ToList();

                if (parentIds.Count > 0)
                {
                    // 查询父菜单及其上级菜单
                    var parentMenus = await Orm.Select<SysMenu>()
                        .Where(a => parentIds.Contains(a.Id))
                        .AsTreeCte(up: true)
                        .ToListAsync();

                    // 合并菜单列表
                    roleMenus.AddRange(parentMenus);
                }

                // 去重并赋值给 RoleMenus
                cachedRoleMenus = roleMenus.DistinctBy(a => a.Id).ToList();
            }

            // 将 RoleMenus 存入缓存，1 分钟后失效
            _memoryCache.Set(roleMenusCacheKey, cachedRoleMenus, TimeSpan.FromMinutes(1));
        }
        RoleMenus = cachedRoleMenus ?? new();
    }

    public SysMenu? CurrentMenu { get; internal set; }
    public bool AuthPathSuccess { get; internal set; }
 
    /// <summary>
    /// 获取组织ID列表
    /// </summary>
    /// <param name="includeChildren">是否包含子部门</param>
    /// <returns>组织ID列表</returns>
    public List<long> GetOrgIds(bool includeChildren)
    {
        // 直接从SysUser获取OrgId
        if (User?.OrgId == null) return new List<long>();

        var orgIds = new List<long>();
        if (includeChildren)
        {
            var childOrgs = Orm.Select<SysOrg>().Where(a => a.Id == User.OrgId).AsTreeCte().ToList();
            orgIds.AddRange(childOrgs.Select(a => a.Id));
        }
        else
        {
            orgIds.Add(User.OrgId);
        }
        return orgIds;
    }

    /// <summary>
    /// 验证指定路径的访问权限
    /// </summary>
    /// <param name="path">要验证的路径</param>
    /// <returns>有权限返回 true，否则返回 false</returns>
    async public Task<bool> AuthPath(string path)
    {
        // 若用户未登录，返回无权限
        if (User == null || string.IsNullOrEmpty(path))
        {
            return false;
        }

        path = path.ToLower().Trim('/');

        // 初始化用户角色和菜单信息
        await InitRoles();
        // 若用户没有角色，返回无权限
        if (!Roles.Any())
        {
            return false;
        }

        // 查询当前路径对应的菜单
        CurrentMenu = RoleMenus.FirstOrDefault(a => a.PathLower == path);

        // 处理路径，转换为小写并去除首尾斜杠
        if (string.IsNullOrEmpty(path) || new[] { "admin/login", "admin/profile" }.Contains(path))
        {
            return true;
        }

        if (CurrentMenu == null && Roles.Any() && new[] { "admin" }.Contains(path))
        {
            return AuthPathSuccess = true;
        }

        if (CurrentMenu != null)
        {
            // 设置当前菜单的父菜单
            CurrentMenu.Parent = RoleMenus.FirstOrDefault(a => a.Id == CurrentMenu.ParentId);
        }

        return AuthPathSuccess = CurrentMenu != null;
    }

    /// <summary>
    /// 验证指定按钮的访问权限
    /// </summary>
    /// <param name="path">要验证的按钮路径</param>
    /// <returns>有权限返回 true，否则返回 false</returns>
    async public Task<bool> AuthButton(string path)
    {
        // 若用户未登录或当前菜单为空，返回无权限
        if (User == null || CurrentMenu == null)
        {
            return false;
        }

        // 初始化用户角色和菜单信息
        await InitRoles();
        // 处理按钮路径，转换为小写
        var pathLower = path.ToLower();
        // 递归查找按钮
        var button = FindButtonRecursive(new List<SysMenu> { CurrentMenu }, pathLower);

        if (button == null)
        {
            // 若用户是管理员，返回有权限
            if (Roles.Any(a => a.IsAdministrator))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 递归查找按钮菜单
    /// </summary>
    /// <param name="findMenus">要查找的菜单列表</param>
    /// <param name="pathLower">按钮路径的小写形式</param>
    /// <returns>找到的按钮菜单，未找到返回 null</returns>
    private SysMenu? FindButtonRecursive(List<SysMenu> findMenus, string pathLower)
    {
        // 使用 HashSet 提升查找性能
        var parentIds = new HashSet<long>(findMenus.Select(m => m.Id));
        var childs = RoleMenus.Where(a => parentIds.Contains(a.ParentId)).ToList();

        // 使用 Count 替代 Any 进行空集合检查
        if (childs.Count == 0)
        {
            return null;
        }

        // 查找符合条件的按钮菜单
        var button = childs.FirstOrDefault(a => a.Type == SysMenuType.按钮 && a.PathLower == pathLower);
        if (button != null)
        {
            return button;
        }

        // 递归查找子菜单
        return FindButtonRecursive(childs, pathLower);
    }

    /// <summary>
    /// 配置 FreeSql
    /// </summary>
    /// <param name="fsql">FreeSql 实例</param>
    internal static void ConfigFreeSql(IFreeSql fsql)
    {
        // 获取服务器时间
        var serverTime = fsql.Ado.QuerySingle(() => DateTime.UtcNow);
        // 计算时间偏移量
        var timeOffset = DateTime.UtcNow.Subtract(serverTime);

        fsql.Aop.AuditValue += (_, e) =>
        {
            if (e.Column.Table.Type == typeof(SysTenant) && e.Column.CsName == nameof(SysTenant.Id))
            {
                // 设置 SysTenant 的 Id 为小写
                e.Value = Convert.ToString(e.Column.Table.ColumnsByCs[nameof(SysTenant.Id)].GetValue(e.Object))?.ToLower();
                return;
            } 

            if (e.Column.CsName == nameof(SysMenu.PathLower) && typeof(SysMenu).IsAssignableFrom(e.Column.Table.Type))
            {
                // 设置 SysMenu 的 PathLower 为小写
                var path = Convert.ToString(e.Column.Table.ColumnsByCs[nameof(SysMenu.Path)].GetValue(e.Object))?.Trim('/');
                e.Column.Table.ColumnsByCs[nameof(SysMenu.Path)].SetValue(e.Object, path);
                e.Value = path?.ToLower();
                return;
            }

            // 数据库时间
            if ((e.Column.CsType == typeof(DateTime) || e.Column.CsType == typeof(DateTime?))
                && e.Column.Attribute.ServerTime != DateTimeKind.Unspecified
                && (e.Value == null || (DateTime)e.Value == default || (DateTime?)e.Value == default))
            {
                // 根据 ServerTime 属性设置时间值
                e.Value = (e.Column.Attribute.ServerTime == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now).Subtract(timeOffset);
                return;
            }

            // 雪花 Id
            if (e.Column.CsType == typeof(long)
                && e.Property.GetCustomAttribute<SnowflakeAttribute>(false) != null
                && (e.Value == null || (long)e.Value == default || (long?)e.Value == default || e.Value?.ToString() == "0"))
            {
                // 生成雪花 Id
                e.Value = YitIdHelper.NextId();
                return;
            }
        };
    }
}