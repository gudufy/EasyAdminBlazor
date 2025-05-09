using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EasyAdminBlazor.Infrastructure;

public interface IEmailService
{
    /// <summary>
    /// 异步发送邮件的方法。
    /// </summary>
    /// <param name="toEmails">收件人的邮箱地址，多个地址用逗号分隔。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="body">邮件正文内容。</param>
    /// <param name="isHtml">指示邮件正文是否为 HTML 格式，默认为 false。</param>
    /// <returns>一个布尔值，发送成功返回 true，失败返回 false。</returns>
    Task<bool> SendEmailAsync(string toEmails, string subject, string body, bool isHtml = false);
}

/// <summary>
/// 默认 SMTP 邮件服务实现，实现了 IEmailService 接口，用于通过 SMTP 协议发送邮件。
/// </summary>
public class SmtpEmailService : IEmailService
{
    // 存储 SMTP 服务器的配置信息
    private readonly IOptionsMonitor<SmtpSettings> _smtpSettings;
    // 用于记录日志的日志记录器
    private readonly ILogger<SmtpEmailService> _logger;

    /// <summary>
    /// 构造函数，初始化 SMTP 邮件服务。
    /// </summary>
    /// <param name="smtpSettings">包含 SMTP 服务器配置信息的选项对象。</param>
    /// <param name="logger">用于记录日志的日志记录器。</param>
    public SmtpEmailService(IOptionsMonitor<SmtpSettings> smtpSettings, ILogger<SmtpEmailService> logger)
    {
        // 获取 SMTP 配置信息
        _smtpSettings = smtpSettings;
        // 初始化日志记录器
        _logger = logger;
    }

    /// <summary>
    /// 异步发送邮件的方法。
    /// </summary>
    /// <param name="toEmails">收件人的邮箱地址，多个地址用逗号分隔。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="body">邮件正文内容。</param>
    /// <param name="isHtml">指示邮件正文是否为 HTML 格式，默认为 false。</param>
    /// <returns>一个布尔值，发送成功返回 true，失败返回 false。</returns>
    public async Task<bool> SendEmailAsync(string toEmails, string subject, string body, bool isHtml = false)
    {
        var setting = _smtpSettings.CurrentValue;

        try
        {
            // 创建 MimeMessage 对象
            var message = new MimeMessage();
            // 设置发件人
            message.From.Add(new MailboxAddress(setting.FromName, setting.FromEmail));

            // 添加收件人
            foreach (var toEmail in toEmails.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
            {
                message.To.Add(new MailboxAddress("Manager", toEmail.Trim()));
            }

            // 设置邮件主题
            message.Subject = subject;

            // 创建邮件正文
            if (isHtml)
            {
                message.Body = new TextPart("html") { Text = body };
            }
            else
            {
                message.Body = new TextPart("plain") { Text = body };
            }

            // 创建 SMTP 客户端
            using (var client = new SmtpClient())
            {
                // 跳过证书验证
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                // 连接到 SMTP 服务器
                await client.ConnectAsync(setting.Server, setting.Port, true);
                // 进行身份验证
                await client.AuthenticateAsync(setting.Username, setting.Password);
                // 发送邮件
                await client.SendAsync(message);
                // 断开连接
                await client.DisconnectAsync(true);
            }

            return true;
        }
        catch (System.Exception ex)
        {
            // 记录异常详细信息，包括发件人邮箱
            _logger.LogError(ex, "邮件发送过程中发生错误，发件人: {FromEmail}", setting.FromEmail);
            return false;
        }
    }
}

/// <summary>
/// 包含 SMTP 服务器配置信息的类。
/// </summary>
public class SmtpSettings
{
    /// <summary>
    /// 获取或设置 SMTP 服务器的地址。
    /// </summary>
    public string Server { get; set; }
    /// <summary>
    /// 获取或设置 SMTP 服务器使用的端口号。
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// 获取或设置用于登录 SMTP 服务器的用户名。
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 获取或设置用于登录 SMTP 服务器的密码。
    /// </summary>
    public string Password { get; set; }
    /// <summary>
    /// 获取或设置发件人的邮箱地址。
    /// </summary>
    public string FromEmail { get; set; }
    /// <summary>
    /// 获取或设置发件人的名称。
    /// </summary>
    public string FromName { get; set; }
    /// <summary>
    /// 获取或设置是否启用 SSL 加密连接到 SMTP 服务器。
    /// </summary>
    public bool EnableSsl { get; set; }
}