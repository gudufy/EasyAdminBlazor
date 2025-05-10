﻿using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EasyAdminBlazor;

/// <summary>
/// 用户密码修改类，用于处理用户密码修改操作。
/// </summary>
public partial class UserPassword
{
    /// <summary>
    /// 获取或设置用户的旧密码。
    /// </summary>
    [Required(ErrorMessage = "{0}不能为空")]
    [DisplayName("旧密码")]
    public string OldPassword { get; set; }
    /// <summary>
    /// 获取或设置用户的新密码。
    /// </summary>
    [Required(ErrorMessage = "{0}不能为空")]
    [DisplayName("新密码")]
    public string NewPassword { get; set; }
    /// <summary>
    /// 获取或设置用户再次输入的新密码，用于验证密码一致性。
    /// </summary>
    [Required(ErrorMessage = "请再输入一次新密码")]
    [Compare(nameof(NewPassword), ErrorMessage = "两次密码不一致")]
    [DisplayName("确认新密码")]
    public string ConfirmPassword { get; set; }
}

/// <summary>
/// 个人资料类，供用户修改自己的资料
/// </summary>
public partial class UserProfile
{
    /// <summary>
    /// 昵称
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("姓名")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string Nickname { get; set; }

    [Column(StringLength = 20)]
    [DisplayName("手机")]
    public string Mobile { get; set; }

    [Column(StringLength = 50)]
    [DisplayName("邮箱")]
    [EmailAddress]
    public string Email { get; set; }
}

/// <summary>
/// 用户
/// </summary>
[Index("uk_username", "Username", true)]
[AutoGenerateClass(Filterable =true)]
public partial class SysUser : EntityFull, IDataPermission
{
    /// <summary>
    /// 名称
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("用户名")]
    [Required(ErrorMessage = "{0}不能为空")]
    [AutoGenerateColumn(Searchable =true)]
    public string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Column(StringLength = 200)]
    [DisplayName("密码")]
    [Required(ErrorMessage = "{0}不能为空")]
    [StringLength(maximumLength: 200, MinimumLength = 6, ErrorMessage = "{0}最少为6个字符")]
    public string Password { get; set; }
    /// <summary>
    /// 昵称
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("姓名")]
    [Required(ErrorMessage = "{0}不能为空")]
    [AutoGenerateColumn(Searchable = true)]
    public string Nickname { get; set; }

    [Column(StringLength = 20)]
    [DisplayName("手机")]
    [AutoGenerateColumn(Searchable = true)]
    public string Mobile { get; set; }

    [Column(StringLength = 50)]
    [DisplayName("邮箱")]
    [AutoGenerateColumn(Searchable = true)]
    public string Email { get; set; }

    [DisplayName("有效")]
    [AutoGenerateColumn(Searchable = true)]
    public bool? IsEnabled { get; set; }

    [DisplayName("所属组织")]
    public long OrgId { get; set; }

    [Navigate("OrgId")]
    [AutoGenerateColumn(Ignore = true)]
    public SysOrg Org { get; set; }

    [Column(StringLength = 500)]
    [AutoGenerateColumn(Searchable = true)]
    [DisplayName("备注")]
    public string Description { get; set; }

    [AutoGenerateColumn(Ignore = true)]
    public bool IsSystem { get; set; }

    /// <summary>
    /// 登陆时间
    /// </summary>
    [DisplayName("最后登录时间")]
    [AutoGenerateColumn(Width = 160,IsVisibleWhenAdd =false,IsVisibleWhenEdit =false)]
    public DateTime LoginTime { get; set; }
}

/// <summary>
/// 登陆日志
/// </summary>
public partial class SysLoginLog : Entity
{
    /// <summary>
    /// 获取或设置用户的登录时间。
    /// </summary>
    [Column(CanUpdate = false, ServerTime = DateTimeKind.Local)]
    public DateTime LoginTime { get; set; }
    /// <summary>
    /// 获取或设置登录用户的用户名。
    /// </summary>
    [Column(StringLength = 50)]
    public string Username { get; set; }
    /// <summary>
    /// 获取或设置登录日志的类型，如登录成功或失败。
    /// </summary>
    public LoginType? Type { get; set; }
    public enum LoginType { 登陆成功, 登陆失败 }
    /// <summary>
    /// 获取或设置用户登录时的 IP 地址。
    /// </summary>
    [Column(StringLength = 50)]
    public string Ip { get; set; }
    /// <summary>
    /// 获取或设置用户登录时所在的城市。
    /// </summary>
    [Column(StringLength = 100)]
    public string City { get; set; }
    /// <summary>
    /// 获取或设置用户登录时使用的浏览器。
    /// </summary>
    public string Browser { get; set; }
    /// <summary>
    /// 获取或设置用户登录时使用的操作系统。
    /// </summary>
    [Column(StringLength = 50)]
    public string? OS { get; set; }
    /// <summary>
    /// 获取或设置用户登录时使用的设备类型。
    /// </summary>
    public WebClientDeviceType Device { get; set; }
    /// <summary>
    /// 获取或设置用户登录时浏览器的语言设置。
    /// </summary>
    [Column(StringLength = 50)]
    public string Language { get; set; }
    /// <summary>
    /// 获取或设置用户登录时的 UserAgent 信息。
    /// </summary>
    public string? UserAgent { get; set; }
    /// <summary>
    /// 获取或设置用户登录时浏览器的引擎信息。
    /// </summary>
    public string? Engine { get; set; }
}