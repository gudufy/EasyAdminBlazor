using BootstrapBlazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Rougamo;
using Rougamo.Context;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method)]
public class OperationLogAttribute : MoAttribute
{
    public string? Description { get; }

    public OperationLogAttribute(string description ="")
    {
        Description = description;
    }

    public override void OnEntry(MethodContext context)
    {
        // 可在方法执行前记录日志相关准备操作，这里暂时留空
    }

    public override void OnSuccess(MethodContext context)
    {
        AddLog(context, OperationResult.成功);
    }

    public override void OnException(MethodContext context)
    {
        AddLog(context, OperationResult.失败);
    }

    private void AddLog(MethodContext context, OperationResult operationResult)
    {
        var service = context.GetServiceProvider();
        var _logService = service.GetService<OperationLogService>();

        _logService?.AddLog(GetOperationType(context,out string operationParams), operationResult, Description??"", operationParams, context.Exception?.Message ?? "");
    }

    /// <summary>
    /// 获取操作类型
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private OperationType? GetOperationType(MethodContext context,out string operationParams)
    {
        operationParams = "";

        if (context.Method.Name.Contains("Delete"))
        {
            operationParams = JsonConvert.SerializeObject(context.Arguments, Formatting.Indented);
            return OperationType.删除;
        }

        if (context.Method.Name.Contains("Query") || context.Method.Name.Contains("Get"))
        {
            return OperationType.查询;
        }

        if (context.Arguments.Length == 2 && (context.Arguments[1] is ItemChangedType changedType))
        {
            operationParams = JsonConvert.SerializeObject(context.Arguments[0], Formatting.Indented);
            return changedType switch
            {
                ItemChangedType.Add => OperationType.添加,
                ItemChangedType.Update => OperationType.修改,
                _ => OperationType.查询
            };
        }

        return null;
    }
}