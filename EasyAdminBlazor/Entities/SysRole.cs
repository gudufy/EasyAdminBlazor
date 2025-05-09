﻿﻿﻿using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

/// <summary>
/// 角色
/// </summary>
public class SysRole : Entity
{
    public enum DataPermissionType
    {
        全部数据,
        本部门及以下数据,
        本部门数据,
        仅本人数据,
        自定义数据
    }

    /// <summary>
    /// 名称
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("角色名称")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string Name { get; set; }

    /// <summary>
    /// 数据权限类型
    /// </summary>
    [DisplayName("数据权限类型")]
    public DataPermissionType DataPermission { get; set; } = DataPermissionType.全部数据;

    /// <summary>
    /// 自定义数据权限(组织ID列表)
    /// </summary>
    [Column(StringLength = -1)]
    [DisplayName("自定义数据权限")]
    public string CustomDataPermission { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    [Column(StringLength = 500)]
    [Display(Name ="描述")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string Description { get; set; }
    /// <summary>
    /// 系统
    /// </summary>
    public bool IsAdministrator { get; set; }

    [Navigate(ManyToMany = typeof(SysRoleUser))]
    public List<SysUser> Users { get; set; }

    [Navigate(ManyToMany = typeof(SysRoleMenu))]
    public List<SysMenu> Menus { get; set; }
}

/// <summary>
/// 角色用户关联类，用于表示角色和用户之间的多对多关系。
/// </summary>
public class SysRoleUser
{
    /// <summary>
    /// 获取或设置角色的 ID。
    /// </summary>
    public long RoleId { get; set; }
    /// <summary>
    /// 获取或设置用户的 ID。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 获取或设置关联的角色。
    /// </summary>
    public SysRole Role { get; set; }
    /// <summary>
    /// 获取或设置关联的用户。
    /// </summary>
    public SysUser User { get; set; }
}

partial class SysMenu
{
    [Navigate(ManyToMany = typeof(SysRoleMenu))]
    public List<SysRole> Roles { get; set; }
    [Navigate(nameof(SysRoleMenu.MenuId))]
    public List<SysRoleMenu> RoleMenus { get; set; }
}
/// <summary>
/// 角色菜单关联类，用于表示角色和菜单之间的多对多关系。
/// </summary>
public class SysRoleMenu
{
    /// <summary>
    /// 获取或设置角色的 ID。
    /// </summary>
    public long RoleId { get; set; }
    /// <summary>
    /// 获取或设置菜单的 ID。
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    /// 获取或设置关联的角色。
    /// </summary>
    public SysRole Role { get; set; }
    /// <summary>
    /// 获取或设置关联的菜单。
    /// </summary>
    public SysMenu Menu { get; set; }
}

partial class SysUser
{
    [Navigate(ManyToMany = typeof(SysRoleUser))]
    [DisplayName("角色")]
    public List<SysRole> Roles { get; set; }

    [Navigate(nameof(SysRoleUser.UserId))]
    public List<SysRoleUser> RoleUsers { get; set; }

    /// <summary>
    /// 获取或设置用户关联的角色 ID 集合。
    /// 该属性会自动同步到 <see cref="Roles"/> 属性。
    /// </summary>
    [Column(IsIgnore = true)]
    [Display(Name ="角色")]
    public IEnumerable<long> RoleIds
    {
        get
        {
            if(Roles == null) return Enumerable.Empty<long>();

            return Roles.Select(x => x.Id);
        }
        set
        {
            if (value == null)
            {
                // If the incoming value is null, set Roles to an empty list
                Roles = new List<SysRole>();
                return;
            }

            if (Roles == null)
            {
                // If Roles is null, initialize it
                Roles = new List<SysRole>();
            }

            // Remove roles that are not in the new ID list
            Roles.RemoveAll(role => !value.Contains(role.Id));

            // Add new roles
            var newRoleIds = value.Except(Roles.Select(role => role.Id));
            foreach (var roleId in newRoleIds)
            {
                // Assume there is a method to create a new SysRole instance based on the ID
                var newRole = new SysRole { Id = roleId };
                if (newRole != null)
                {
                    Roles.Add(newRole);
                }
            }
        }
    }
}