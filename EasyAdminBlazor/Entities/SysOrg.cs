using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace EasyAdminBlazor;

public class SysOrg : Entity, IHasParentId<long>
{
    public enum OrgType
    {
        集团,
        公司,
        部门,
        供应商
    }

    [Navigate("ParentId")]
    /// <summary>
    /// 获取或设置父组织对象。
    /// </summary>
    public SysOrg Parent { get; set; }

    [Navigate("ParentId")]
    /// <summary>
    /// 获取或设置子组织列表。
    /// </summary>
    public List<SysOrg> Childs { get; set; }

    [Required(ErrorMessage = "{0}不能为空")]
    [DisplayName("组织名称")]
    [Column(StringLength = 50)]
    /// <summary>
    /// 获取或设置组织的名称。
    /// </summary>
    public string Label { get; set; }

    [DisplayName("类型")]
    /// <summary>
    /// 获取或设置组织的类型。
    /// </summary>
    public OrgType Type { get; set; }

    [DisplayName("所属上级")]
    /// <summary>
    /// 获取或设置父组织的 ID。
    /// </summary>
    public long ParentId { get; set; }

    [DisplayName("排序")]
    /// <summary>
    /// 获取或设置组织的排序值。
    /// </summary>
    public int Sort { get; set; }

    [DisplayName("有效")]
    /// <summary>
    /// 获取或设置一个值，指示该组织是否有效。
    /// </summary>
    public bool IsEnabled { get; set; }

    [Column(StringLength = 500)]
    [DisplayName("备注")]
    /// <summary>
    /// 获取或设置组织的备注信息。
    /// </summary>
    public string Description { get; set; }
}