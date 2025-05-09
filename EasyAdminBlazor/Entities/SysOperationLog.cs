using FreeSql.DataAnnotations;

public class SysOperationLog:EntityCreated<long>
{
    // 操作页面
    [Column(StringLength = 100)]
    public string Path { get; set; }

    // 操作描述
    [Column(StringLength = 200)]
    public string OperationDescription { get; set; }

    // 操作参数
    [Column(StringLength = -1)]
    public string OperationParams { get; set; }

    // IP 地址
    [Column(StringLength = 45)]
    public string ClientIp { get; set; }

    // 客户端设备信息
    [Column(StringLength = 500)]
    public string ClientDevice { get; set; }

    // 操作结果，如 Success、Failure 等
    public OperationResult? OperationResult { get; set; }

    // 失败原因
    [Column(StringLength = -1)]
    public string FailureReason { get; set; }
}

public enum OperationResult
{
    成功, 失败
}

public enum OperationType
{
    查询,添加,修改,删除,导入,导出
}