using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using Microsoft.Extensions.Localization;
using MiniExcelLibs.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EasyAdminBlazor;

public interface IEntityCreated
{
    /// <summary>
    /// 创建者用户Id
    /// </summary>
    long? CreatedUserId { get; set; }
    /// <summary>
    /// 创建者
    /// </summary>
    string CreatedUserName { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime? CreatedTime { get; set; }
}

/// <summary>
/// 实体创建
/// </summary>
public abstract class EntityCreated<TKey> : Entity<TKey>, IEntityCreated
{
    /// <summary>
    /// 创建者Id
    /// </summary>
    [Column(Position = -22, CanUpdate = false)]
    [ExcelColumn(Ignore = true)]
    public virtual long? CreatedUserId { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    [Column(Position = -21, CanUpdate = false), MaxLength(50)]
    [DisplayName("创建人")]
    public virtual string CreatedUserName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column(Position = -20, CanUpdate = false, ServerTime = DateTimeKind.Local)]
    [DisplayName("创建时间")]
    public virtual DateTime? CreatedTime { get; set; }
}

/// <summary>
/// 实体创建
/// </summary>
public abstract class EntityCreated : EntityCreated<long>
{
}
