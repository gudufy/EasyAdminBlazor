using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class SysLog : Entity<long>
{
    /// <summary>
    /// 异常时间
    /// </summary>
    [DisplayName("异常时间")]
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 日志级别，最大长度 50
    /// </summary>
    [Column(StringLength = 50)]
    [DisplayName("日志级别")]
    public string LogLevel { get; set; }

    /// <summary>
    /// 日志类别，最大长度 200
    /// </summary>
    [Column(StringLength = 200)]
    [DisplayName("日志类别")]
    public string Category { get; set; }

    /// <summary>
    /// 日志消息，最大长度 2000
    /// </summary>
    [Column(StringLength = 2000)]
    [DisplayName("日志消息")]
    public string Message { get; set; }

    /// <summary>
    /// 异常信息，最大长度 4000
    /// </summary>
    [Column(StringLength = 4000)]
    [DisplayName("异常信息")]
    public string Exception { get; set; }
}