﻿using BootstrapBlazor.Components;
using FreeSql;
using FreeSql.DataAnnotations;

/// <summary>
/// 租户
/// </summary>
public partial class SysTenant : EntityCreated<string>
{
    /// <summary>
    /// 获取或设置数据库的类型。
    /// </summary>
    public DataType DataType { get; set; }

    /// <summary>
    /// 获取或设置数据库的连接字符串。
    /// </summary>
    [Column(StringLength = 500)]
    public string ConenctionString { get; set; }

    /// <summary>
    /// 域名
    /// </summary>
    [Column(StringLength = 50)]
    public string Host { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 说明
    /// </summary>
    [Column(StringLength = 500)]
    public string Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    [Navigate(ManyToMany = typeof(SysTenantMenu))]
    public List<SysMenu> Menus { get; set; }
}
partial class SysMenu
{
    [Navigate(ManyToMany = typeof(SysTenantMenu))]
    public List<SysTenant> Tenants { get; set; }
}
/// <summary>
/// 租户与菜单关联类，用于表示租户和菜单之间的多对多关系。
/// </summary>
public class SysTenantMenu
{
    /// <summary>
    /// 获取或设置租户的 ID。
    /// </summary>
    public string TenantId { get; set; }
    /// <summary>
    /// 获取或设置菜单的 ID。
    /// </summary>
    public long MenuId { get; set; }

    /// <summary>
    /// 获取或设置关联的租户。
    /// </summary>
    public SysTenant Tenant { get; set; }
    /// <summary>
    /// 获取或设置关联的菜单。
    /// </summary>
    public SysMenu Menu { get; set; }
}