using EasyAdminBlazor.Infrastructure;
using EasyAdminBlazor.Infrastructure.Configuration;
using EasyAdminBlazor.Infrastructure.Encrypt;
using EasyAdminBlazor.Infrastructure.Logger;
using EasyAdminBlazor.Services;
using FreeSql;
using FreeSql.Aop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Web;
using Yitter.IdGenerator;
using Console = System.Console;

/// <summary>
/// 管理 EasyAdminBlazor 相关配置的选项类
/// </summary>
public class EasyAdminBlazorOptions
{
    /// <summary>
    /// 工作节点 ID，默认值为 1
    /// </summary>
    public ushort WorkId { get; set; } = 1;
    /// <summary>
    /// 程序集数组，用于指定扫描的程序集
    /// </summary>
    public Assembly[] Assemblies { get; set; }
}

/// <summary>
/// 包含 EasyAdminBlazor 服务扩展方法的静态类
/// </summary>
public static class AdminExtensions
{
    /// <summary>
    /// 向服务集合中添加 EasyAdminBlazor 相关服务
    /// </summary>
    /// <param name="builder">服务集合</param>
    /// <param name="options">EasyAdminBlazor 配置选项</param>
    /// <returns>添加服务后的服务集合</returns>
    public static WebApplicationBuilder AddEasyAdminBlazor(this WebApplicationBuilder builder, EasyAdminBlazorOptions options)
    {
        // 若选项为空，则使用默认选项
        if (options == null) options = new EasyAdminBlazorOptions();

        // 若未指定程序集，则使用包含 AdminExtensions 的程序集
        if (options.Assemblies == null)
            options.Assemblies = new[] { typeof(AdminExtensions).Assembly };
        else
            // 合并程序集并去重
            options.Assemblies = options.Assemblies.Concat(new[] { typeof(AdminExtensions).Assembly }).Distinct().ToArray();

        // 初始化 YitIdHelper 雪花 ID 生成器
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions(options.WorkId) { WorkerIdBitLength = 6 });

        // 定义 FreeSql 工厂方法
        Func <IServiceProvider, IFreeSql> fsqlFactory = r =>
        {
            // 构建 FreeSql 实例
            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.MySql, @"Data Source=127.0.0.1;Port=3306;User ID=root;Password=123456; Initial Catalog=freesql;Charset=utf8mb4; SslMode=none;Min pool size=1")
                // 监控 SQL 命令并输出到控制台
                .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))
                .UseNoneCommandParameter(false)
                // 自动同步实体结构到数据库，仅在 CRUD 操作时生成表
                .UseAutoSyncStructure(true) 
                .Build();

            // 使用 JSON 映射
            fsql.UseJsonMap();

            // 配置 FreeSql
            AdminContext.ConfigFreeSql(fsql);

            // 从数据库加载配置
            builder.Configuration.AddDatabaseConfiguration(fsql);

            #region 初始化数据
            if (fsql.Select<SysMenu>().Any() == false)
            {
                List<SysMenu> getCudButtons(params SysMenu[] btns) => new[]
                {
                new SysMenu { Label = "添加", Path = "add", Sort = 10011, Type = SysMenuType.按钮, IsSystem = true, },
                new SysMenu { Label = "编辑", Path = "edit", Sort = 10012, Type = SysMenuType.按钮, IsSystem = true, },
                new SysMenu { Label = "删除", Path = "remove", Sort = 10013, Type = SysMenuType.按钮, IsSystem = true, }
            }.Concat(btns ?? new SysMenu[0]).ToList();
                var repo = fsql.GetAggregateRootRepository<SysMenu>();
                repo.Insert(new[]
                {
                new SysMenu
                {
                    Label = "系统管理",
                    Icon = "fa-code",
                    Path = "",
                    Sort = 1001,
                    Type = SysMenuType.菜单,
                    IsSystem = true,
                    Childs = new List<SysMenu>
                    {
                        new SysMenu {
                            Label = "组织", Path = "Admin/Org", Sort = 201, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons(Array.Empty<SysMenu>())
                        },
                        new SysMenu { Label = "用户", Path = "Admin/User", Sort = 10001, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons(Array.Empty<SysMenu>())
                        },
                        new SysMenu { Label = "角色", Path = "Admin/Role", Sort = 10002, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons(
                            new SysMenu { Label = "分配菜单", Path = "alloc_menus", Sort = 10015, Type = SysMenuType.按钮, IsSystem = true, })
                        },
                        new SysMenu { Label = "菜单", Path = "Admin/Menu", Sort = 10003, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons() },
                        new SysMenu { Label = "数据字典", Path = "Admin/Dict", Sort = 10005, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons() },
                        //new SysMenu { Label = "租户", Path = "Admin/Tenant", Sort = 10006, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons() },
                        new SysMenu {
                            Label = "参数配置", Path = "Admin/Config", Sort = 207, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons(Array.Empty<SysMenu>())
                        },
                        new SysMenu { Label = "文件管理", Path = "Admin/File", Sort = 10008, Type = SysMenuType.菜单, IsSystem = true, Childs = getCudButtons()
                        },
                    }
                },
                new SysMenu
                {
                    Label = "系统日志",
                    Icon = "fa-server",
                    Path = "",
                    Sort = 1002,
                    Type = SysMenuType.菜单,
                    IsSystem = true,
                    Childs = new List<SysMenu>
                    {
                        new SysMenu {
                            Label = "登录日志", Path = "Admin/LoginLog", Sort = 10002, Type = SysMenuType.菜单, IsSystem = true
                        },
                        new SysMenu { Label = "操作日志", Path = "Admin/OperationLog", Sort = 10001, Type = SysMenuType.菜单, IsSystem = true
                        },
                        new SysMenu { Label = "错误日志", Path = "Admin/ErrorLog", Sort = 10003, Type = SysMenuType.菜单, IsSystem = true, Childs = new List<SysMenu>{ new SysMenu { Label = "删除", Path = "remove", Sort = 10013, Type = SysMenuType.按钮, IsSystem = true, } }
                        },
                    }
                },
            });
            }

            if (fsql.Select<SysRole>().Where(a => a.IsAdministrator).Any() == false)
                fsql.Insert(new SysRole { Name = "Administrator", Description = "管理员角色", IsAdministrator = true }).ExecuteAffrows();
            if (fsql.Select<SysOrg>().Any() == false)
                fsql.Insert(new SysOrg { Label = "总公司", Description = "总部" }).ExecuteAffrows();
            if (fsql.Select<SysUser>().Where(a => a.Roles.Any(b => b.IsAdministrator)).Any() == false)
            {
                var adminUser = new SysUser { Username = "admin", Password = PBKDF2Encrypt.HashPassword("123yyq"), Nickname = "管理员",IsEnabled=true };
                adminUser.Roles = [fsql.Select<SysRole>().Where(a => a.IsAdministrator).First()];
                adminUser.OrgId = fsql.Select<SysOrg>().First().Id;
                fsql.GetAggregateRootRepository<SysUser>().Insert(adminUser);
            }
            if (fsql.Select<SysConfig>().Where(a=>a.Code.Contains(":")).Any() == false)
            {
                var configEntitys= new List<SysConfig>();
                var smtpSettings = builder.Configuration.GetSection("SmtpSettings").GetChildren();
                foreach (var setting in smtpSettings)
                {
                    configEntitys.Add(new SysConfig
                    {
                        Code = "SmtpSettings:" + setting.Key,
                        Content = setting.Value,
                        IsSystem = true
                    });
                }
                var smsSettings = builder.Configuration.GetSection("Aliyun:Sms").GetChildren();
                foreach (var setting in smsSettings)
                {
                    configEntitys.Add(new SysConfig
                    {
                        Code = "Aliyun:Sms:" + setting.Key,
                        Content = setting.Value,
                        IsSystem = true
                    });
                }
                fsql.GetAggregateRootRepository<SysConfig>().Insert(configEntitys);
            }
            #endregion

            return fsql;
        };

        // 注册 FreeSql 工厂为单例服务
        builder.Services.AddSingleton(fsqlFactory);
        // 注册 AdminContext 为作用域服务
        builder.Services.AddScoped<AdminContext>();
        // 注册工作单元管理器为作用域服务
        builder.Services.AddScoped<UnitOfWorkManager>();
        // 注册仓储选项为作用域服务
        builder.Services.AddScoped(r => new RepositoryOptions
        {
            AuditValue = e => {
                // 获取 AdminContext 中的用户信息
                var user = r.GetService<AdminContext>()?.User;
                if (user == null) return;

                // 插入操作时，设置创建用户信息
                if (e.AuditValueType == AuditValueType.Insert &&
                    e.Object is IEntityCreated obj1 && obj1 != null)
                {
                    obj1.CreatedUserId = user.Id;
                    obj1.CreatedUserName = user.Username;
                }

                // 更新操作时，设置修改用户信息
                if (e.AuditValueType == AuditValueType.Update &&
                    e.Object is IEntityModified obj2 && obj2 != null)
                {
                    obj2.ModifiedUserId = user.Id;
                    obj2.ModifiedUserName = user.Username;
                }

                // 用户部门
                if (e.AuditValueType == AuditValueType.Insert &&
                    e.Object is IDataPermission obj3 && obj3 != null)
                {
                    obj3.OrgId = user.OrgId;
                    return;
                }
            }
        });

        // 注册基础仓储相关服务
        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BasicRepository<>));
        builder.Services.AddScoped(typeof(BaseRepository<>), typeof(BasicRepository<>));
        builder.Services.AddScoped(typeof(IBaseRepository<,>), typeof(BasicRepository<,>));
        builder.Services.AddScoped(typeof(BaseRepository<,>), typeof(BasicRepository<,>));
        // 注册聚合根仓储服务
        builder.Services.AddScoped(typeof(IAggregateRootRepository<>), typeof(DddRepository<>));
        // 注册文件服务
        builder.Services.AddScoped<FileService>();
        builder.Services.AddScoped<OperationLogService>();
        // 注册邮件发送服务SMTP
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
        builder.Services.AddTransient<IEmailService, SmtpEmailService>();
        // 配置阿里云短信服务
        builder.Services.Configure<AliyunSmsSettings>(builder.Configuration.GetSection("Aliyun:Sms"));
        builder.Services.AddSingleton<ISmsService>(provider =>
        {
            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<AliyunSmsSettings>>();
            var logger = provider.GetRequiredService<ILogger<AliyunSmsService>>();
            return new AliyunSmsService(optionsMonitor, logger);
        });

        // 注册日志服务并添加 DatabaseLoggerProvider
        builder.Services.AddLogging(loggingBuilder =>
        {
            // 获取 IFreeSql 实例
            var orm = builder.Services.BuildServiceProvider().GetRequiredService<IFreeSql>();
            // 添加 DatabaseLoggerProvider
            loggingBuilder.AddProvider(new DatabaseLoggerProvider(() =>new DatabaseLoggerConfiguration
            {
                LogLevel = LogLevel.Error
            },orm));
        });

        // 添加 HttpContext 访问器
        builder.Services.AddHttpContextAccessor();
        // 添加 BootstrapBlazor 服务
        builder.Services.AddBootstrapBlazor();

        return builder;
    }

    /// <summary>
    /// 从导航管理器的 URI 中获取指定名称的查询字符串值
    /// </summary>
    /// <param name="nav">导航管理器</param>
    /// <param name="name">查询字符串名称</param>
    /// <returns>查询字符串值，若不存在则返回空字符串</returns>
    public static string GetQueryStringValue(this NavigationManager nav, string name)
    {
        // 解析导航管理器 URI 中的查询字符串
        var obj = HttpUtility.ParseQueryString(new Uri(nav.Uri).Query);
        return obj[name] ?? "";
    }
}