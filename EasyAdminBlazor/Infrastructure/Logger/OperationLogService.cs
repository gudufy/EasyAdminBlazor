using FreeSql;
using Microsoft.AspNetCore.Components;
using Rougamo.Context;
using System.Threading.Tasks;

namespace EasyAdminBlazor;

public class OperationLogService
{
    private readonly IAggregateRootRepository<SysOperationLog> _repo;
    private readonly IAggregateRootRepository<SysMenu> _repoMenu;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly NavigationManager _navigationManager;

    public OperationLogService(IAggregateRootRepository<SysOperationLog> repo, IAggregateRootRepository<SysMenu> repoMenu, IHttpContextAccessor httpContextAccessor, NavigationManager navigationManager)
    {
        _repo = repo;
        _repoMenu = repoMenu;
        _httpContextAccessor = httpContextAccessor;
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// 添加操作日志
    /// </summary>
    /// <param name="operationType">操作类型</param>
    /// <param name="operationResult">操作结果</param>
    /// <param name="operationDescription">操作描述</param>
    /// <param name="operationParams">参数</param>
    /// <param name="failureReason">异常信息</param>
    /// <returns></returns>
    public async Task AddLog(OperationType? operationType, OperationResult operationResult, string operationDescription="",string operationParams="", string failureReason = "")
    {
        // 提前验证必要依赖是否为空，避免后续空引用异常
        if (_httpContextAccessor?.HttpContext == null || _navigationManager == null || _repo == null)
        {
            // 可以根据实际情况记录错误日志或者抛出异常
            return;
        }

        // 获取客户端 IP 和设备信息
        string clientIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        string clientDevice = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString() ?? "";

        // 创建日志对象
        var log = new SysOperationLog
        {
            OperationResult = operationResult,
            ClientIp = clientIp,
            ClientDevice = clientDevice,
            FailureReason = failureReason,
            OperationParams = operationParams,
            Path = _navigationManager.ToBaseRelativePath(_navigationManager.Uri)
        };

        // 处理操作描述
        if (string.IsNullOrEmpty(operationDescription) && !string.IsNullOrEmpty(log.Path) && _repoMenu != null)
        {
            // 使用异步方法查询菜单，避免阻塞线程
            var menu = await _repoMenu.Select.Where(x => x.Path == log.Path).FirstAsync();
            if (menu != null)
            {
                log.OperationDescription = $"{operationType}{menu.Label}";
            }
        }
        else
        {
            log.OperationDescription = operationDescription.StartsWith("{0}")
                ? string.Format(operationDescription, operationType)
                : operationDescription;
        }

        // 插入日志记录
        await _repo.InsertAsync(log);
    }
}