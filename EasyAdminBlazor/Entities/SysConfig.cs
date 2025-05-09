﻿using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyAdminBlazor;

/// <summary>
/// 系统设置
/// </summary>
[Index("uk_code", "Code", true)]
public class SysConfig : EntityFull
{
    /// <summary>
    /// 是否系统。指示该配置是否为系统级配置。
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 名称。该配置的名称。
    /// </summary>
    [Column(StringLength = 200)]
    [DisplayName("名称")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string Name { get; set; }

    /// <summary>
    /// 唯一码。该配置的唯一标识代码。
    /// </summary>
    [Column(StringLength = 30)]
    [DisplayName("唯一码")]
    [Required(ErrorMessage = "{0}不能为空")]
    public string Code { get; set; }

    /// <summary>
    /// 内容。该配置的具体内容。
    /// </summary>
    [Column(StringLength = 500)]
    [DisplayName("值")]
    public string Content { get; set; }

    /// <summary>
    /// 该配置的相关说明信息
    /// </summary>
    [Column(StringLength = 200)]
    [DisplayName("说明")]
    public string Rmark { get; set; }
}