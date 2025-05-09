﻿using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace EasyAdminBlazor;

/// <summary>
/// 菜单
/// </summary>
public partial class SysMenu : EntityCreated, IHasParentId<long>
{
    /// <summary>
    /// 获取或设置父菜单对象。
    /// </summary>
    [Navigate(nameof(ParentId))]
    public SysMenu Parent { get; set; }

    /// <summary>
    /// 获取或设置子菜单列表。
    /// </summary>
    [Navigate(nameof(ParentId))]
    [JsonIgnore]
    public List<SysMenu> Childs { get; set; }

    /// <summary>
    /// 获取或设置菜单操作按钮列表。
    /// </summary>
    [Column(IsIgnore = true)]
    [DisplayName("按钮")]
    public IEnumerable<string> Buttons { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示该菜单是否为系统菜单。
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 获取或设置一个值，指示该菜单是否隐藏。
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// 获取或设置父菜单的 ID。
    /// </summary>
    [DisplayName("父级菜单")]
    public long ParentId { get; set; }

    /// <summary>
    /// 获取或设置菜单的名称。
    /// </summary>
    [Column(StringLength = 50)]
    [Required(ErrorMessage = "{0}不能为空")]
    [DisplayName("菜单名称")]
    public string Label { get; set; }

    /// <summary>
    /// 获取或设置菜单的图标。
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("图标")]
    public string Icon { get; set; }

    /// <summary>
    /// 获取或设置菜单的路径。
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("路径")]
    public string Path { get; set; }

    /// <summary>
    /// 获取或设置菜单的小写路径。
    /// </summary>
    [Column(StringLength = 50)]
    public string PathLower { get; set; }

    /// <summary>
    /// 获取或设置菜单的排序值。
    /// </summary>
    [DisplayName("排序")]
    public int Sort { get; set; }

    /// <summary>
    /// 获取或设置菜单的类型。
    /// </summary>
    [DisplayName("类型")]
    public SysMenuType Type { get; set; }
}

public enum SysMenuType { 菜单, 按钮, 外部连接, 增删改查 }