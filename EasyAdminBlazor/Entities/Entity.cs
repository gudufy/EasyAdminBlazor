﻿using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using MiniExcelLibs.Attributes;
using System.ComponentModel;

namespace EasyAdminBlazor;

/// <summary>
/// 定义实体接口，包含主键属性。
/// </summary>
/// <typeparam name="TKey">主键的类型。</typeparam>
public interface IEntity<TKey>
{
    /// <summary>
    /// 获取或设置实体的主键 Id。
    /// </summary>
    TKey Id { get; set; }
}

/// <summary>
/// 实体的抽象基类，实现了 <see cref="IEntity{TKey}"/> 接口。
/// </summary>
/// <typeparam name="TKey">主键的类型。</typeparam>
public abstract class Entity<TKey> : IEntity<TKey>
{
    /// <summary>
    /// 获取或设置实体的主键 Id。
    /// </summary>
    [Snowflake]
    [Column(Position = 1, IsIdentity = false, IsPrimary = true)]
    public virtual TKey Id { get; set; }
}

/// <summary>
/// 实体的抽象基类，使用 <see cref="long"/> 作为主键类型。
/// </summary>
public abstract class Entity : Entity<long>
{
}