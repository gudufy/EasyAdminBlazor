using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EasyAdminBlazor;

/// <summary>
/// 文件
/// </summary>
public partial class SysFile : EntityCreated
{
    /// <summary>
    /// 获取或设置文件存储服务提供商。
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// 文件存储所在的存储桶名称
    /// </summary>
    [Column(StringLength = 200)]
    public string BucketName { get; set; }

    /// <summary>
    /// 文件在存储系统中的目录路径
    /// </summary>
    [Column(StringLength = 500)]
    public string FileDirectory { get; set; }

    /// <summary>
    /// 文件的全局唯一标识符
    /// </summary>
    public Guid FileGuid { get; set; }

    /// <summary>
    /// 文件在存储系统中保存的名称
    /// </summary>
    [Column(StringLength = 200)]
    public string SaveFileName { get; set; }

    /// <summary>
    /// 文件的原始名称
    /// </summary>
    [Column(StringLength = 200)]
    [DisplayName("文件名")]
    public string FileName { get; set; }

    /// <summary>
    /// 文件的扩展名，用于标识文件类型。
    /// </summary>
    [Column(StringLength = 20)]
    [DisplayName("扩展名")]
    public string Extension { get; set; }

    /// <summary>
    /// 文件的字节大小
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 格式化后的文件大小，方便阅读。
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("文件大小")]
    public string SizeFormat { get; set; }

    /// <summary>
    /// 文件的访问链接地址
    /// </summary>
    [Column(StringLength = 500)]
    public string LinkUrl { get; set; }

    /// <summary>
    /// 文件的 MD5 哈希值，用于校验文件完整性和防止重复上传。
    /// </summary>
    [Column(StringLength = 50)]
    public string Md5 { get; set; } = string.Empty;

    [Column(IsIgnore = true)]
    /// <summary>
    /// 获取或设置一个值，指示该文件是否被选中。
    /// </summary>
    public bool IsSelect { get; set; }
}