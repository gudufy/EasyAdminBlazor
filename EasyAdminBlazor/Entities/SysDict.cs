﻿using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// 数据字典
/// </summary>
public partial class SysDict : Entity
{
     /// <summary>
     /// 获取或设置字典类型的父级 ID。
     /// </summary>
      public long ParentId { get; set; }
  
      [Navigate("ParentId")]
      public SysDict Parent { get; set; }
  
     /// <summary>
     /// 获取或设置字典的编码。
     /// </summary>
      [Column(StringLength = 50)]
      [DisplayName("编码")]
      [Required(ErrorMessage ="{0}不能为空")]
      public string Name { get; set; }
  
     /// <summary>
     /// 获取或设置字典的值。
     /// </summary>
      [Column(StringLength = 50)]
      [DisplayName("值")]
      public string Value { get; set; }
  
     /// <summary>
     /// 获取或设置字典的描述信息。
     /// </summary>
      [Column(StringLength = 500)]
      [DisplayName("描述")]
      public string Description { get; set; }
  
     /// <summary>
     /// 获取或设置字典是否启用。
     /// </summary>
      public bool Enabled { get; set; } = true;
  
     /// <summary>
     /// 获取或设置字典的排序值。
     /// </summary>
      public int Sort { get; set; }
  }