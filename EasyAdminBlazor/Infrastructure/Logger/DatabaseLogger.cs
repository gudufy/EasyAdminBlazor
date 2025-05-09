using FreeSql;
using Microsoft.Extensions.Logging;
using System;

namespace EasyAdminBlazor.Infrastructure.Logger;

/// <summary>
/// 自定义数据库日志记录器，实现 ILogger 接口，将日志信息记录到数据库
/// </summary>
public class DatabaseLogger : ILogger
{
    /// <summary>
    /// 日志记录器的名称，通常为日志来源类别
    /// </summary>
    private readonly string _name;
    /// <summary>
    /// 用于获取数据库日志记录器配置的委托
    /// </summary>
    private readonly Func<DatabaseLoggerConfiguration> _getCurrentConfig;
    /// <summary>
    /// FreeSql 实例，用于执行数据库操作
    /// </summary>
    private readonly IFreeSql _freesql;

    /// <summary>
    /// 数据库日志记录器的构造函数
    /// </summary>
    /// <param name="name">日志记录器的名称</param>
    /// <param name="getCurrentConfig">获取数据库日志记录器配置的委托</param>
    /// <param name="freesql">FreeSql 实例</param>
    public DatabaseLogger(string name, Func<DatabaseLoggerConfiguration> getCurrentConfig, IFreeSql freesql)
    {
        _name = name;
        _getCurrentConfig = getCurrentConfig;
        _freesql = freesql;
    }

    /// <summary>
    /// 开始一个逻辑操作范围
    /// </summary>
    /// <typeparam name="TState">范围状态的类型</typeparam>
    /// <param name="state">要用于范围的状态</param>
    /// <returns>一个实现 IDisposable 的对象，用于在范围结束时释放资源</returns>
    public IDisposable BeginScope<TState>(TState state)
    {
        // 当前未实现范围功能，直接返回 null
        return null;
    }

    /// <summary>
    /// 检查指定的日志级别是否启用
    /// </summary>
    /// <param name="logLevel">要检查的日志级别</param>
    /// <returns>如果启用则返回 true，否则返回 false</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        // 比较配置中的日志级别和传入的日志级别，判断是否启用
        return _getCurrentConfig().LogLevel <= logLevel;
    }

    /// <summary>
    /// 记录日志信息到数据库
    /// </summary>
    /// <typeparam name="TState">日志状态的类型</typeparam>
    /// <param name="logLevel">日志级别</param>
    /// <param name="eventId">事件 ID</param>
    /// <param name="state">日志状态</param>
    /// <param name="exception">异常信息（如果有）</param>
    /// <param name="formatter">用于格式化日志消息的委托</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        // 检查当前日志级别是否启用，如果未启用则直接返回
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // 获取当前的数据库日志记录器配置
        var config = _getCurrentConfig();
        // 使用传入的格式化委托生成日志消息
        var message = formatter(state, exception);

        try
        {
            // 异步插入日志记录到数据库
            _freesql.Insert(new SysLog
            {
                LogLevel = logLevel.ToString(), // 日志级别
                Category = _name, // 日志来源类别
                Message = message, // 日志消息
                Exception = exception?.ToString(), // 异常信息
                CreatedTime = DateTime.Now // 日志创建时间
            }).ExecuteAffrowsAsync();
        }catch(Exception ex)
        {
            Console.WriteLine("日志记录异常："+ex.Message);
        }
    }
}

/// <summary>
/// 数据库日志记录器的配置类
/// </summary>
public class DatabaseLoggerConfiguration
{
    /// <summary>
    /// 日志记录级别，默认为 Information
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}

/// <summary>
/// 自定义数据库日志记录器提供程序，实现 ILoggerProvider 接口，用于创建 DatabaseLogger 实例
/// </summary>
public class DatabaseLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// 用于获取数据库日志记录器配置的委托
    /// </summary>
    private readonly Func<DatabaseLoggerConfiguration> _getCurrentConfig;
    /// <summary>
    /// FreeSql 实例，用于执行数据库操作
    /// </summary>
    private readonly IFreeSql _freesql;
    /// <summary>
    /// 用于线程同步的对象
    /// </summary>
    private readonly object _sync = new object();
    /// <summary>
    /// 指示该提供程序是否已被释放
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// 数据库日志记录器提供程序的构造函数
    /// </summary>
    /// <param name="getCurrentConfig">获取数据库日志记录器配置的委托</param>
    /// <param name="freesql">FreeSql 实例</param>
    public DatabaseLoggerProvider(Func<DatabaseLoggerConfiguration> getCurrentConfig, IFreeSql freesql)
    {
        _getCurrentConfig = getCurrentConfig;
        _freesql = freesql;
    }

    /// <summary>
    /// 创建一个新的 ILogger 实例
    /// </summary>
    /// <param name="categoryName">日志记录器的类别名称</param>
    /// <returns>新的 ILogger 实例</returns>
    public ILogger CreateLogger(string categoryName)
    {
        // 检查该提供程序是否已被释放，如果已释放则抛出异常
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DatabaseLoggerProvider));
        }

        // 创建并返回一个新的 DatabaseLogger 实例
        return new DatabaseLogger(categoryName, _getCurrentConfig, _freesql);
    }

    /// <summary>
    /// 释放该提供程序占用的资源
    /// </summary>
    public void Dispose()
    {
        lock (_sync)
        {
            // 检查该提供程序是否还未被释放，如果未释放则标记为已释放
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}