using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace EasyAdminBlazor.Infrastructure;
public interface ISmsService
{
    /// <summary>
    /// 发送短信
    /// </summary>
    /// <param name="phoneNumbers">接收短信的手机号码，多个号码用逗号分隔</param>
    /// <param name="templateCode">短信模板编码</param>
    /// <param name="templateParam">短信模板变量对应的实际值，JSON 格式</param>
    /// <returns>发送结果，成功返回 true，失败返回 false</returns>
    Task<bool> SendSmsAsync(string phoneNumbers, string templateCode, string templateParam);
}

public class AliyunSmsService : ISmsService
{
    private readonly IOptionsMonitor<AliyunSmsSettings> _optionsMonitor;
    private readonly ILogger<AliyunSmsService> _logger; // 日志记录器

    /// <summary>
    /// 阿里云短信服务构造函数
    /// </summary>
    /// <param name="accessKeyId">阿里云访问密钥 ID</param>
    /// <param name="accessKeySecret">阿里云访问密钥 Secret</param>
    /// <param name="signName">短信签名名称</param>
    /// <param name="logger">日志记录器</param>
    public AliyunSmsService(IOptionsMonitor<AliyunSmsSettings> optionsMonitor, ILogger<AliyunSmsService> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    /// <summary>
    /// 异步发送短信的具体实现
    /// </summary>
    /// <param name="phoneNumbers">接收短信的手机号码，多个号码用逗号分隔</param>
    /// <param name="templateCode">短信模板代码</param>
    /// <param name="templateParam">短信模板参数，JSON 格式字符串</param>
    /// <returns>发送成功返回 true，失败返回 false</returns>
    public async Task<bool> SendSmsAsync(string phoneNumbers, string templateCode, string templateParam)
    {
        var setting = _optionsMonitor.CurrentValue;
        try
        {
            // 构建请求参数
            var parameters = new Dictionary<string, string>
                {
                    { "Format", "JSON" }, // 响应格式为 JSON
                    { "Version", "2017-05-25" }, // 短信服务 API 版本
                    { "AccessKeyId", setting.AccessKeyId }, // 阿里云访问密钥 ID
                    { "SignatureMethod", "HMAC-SHA1" }, // 签名方式
                    { "Timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") }, // 请求时间戳
                    { "SignatureVersion", "1.0" }, // 签名版本
                    { "SignatureNonce", Guid.NewGuid().ToString() }, // 唯一随机数
                    { "Action", "SendSms" }, // 请求操作，发送短信
                    { "PhoneNumbers", phoneNumbers }, // 接收短信的手机号码
                    { "SignName", setting.SignName }, // 短信签名名称
                    { "TemplateCode", templateCode }, // 短信模板代码
                    { "TemplateParam", templateParam } // 短信模板参数
                };

            // 计算签名并添加到请求参数中
            parameters["Signature"] = ComputeSignature(parameters, setting.AccessKeySecret);

            // 构建请求 URL
            var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={UrlEncode(p.Value)}"));
            var requestUrl = $"https://dysmsapi.aliyuncs.com/?{queryString}";

            // 使用 HttpClient 发送 GET 请求
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    // 读取响应内容
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (responseContent.Contains("\"Code\":\"OK\""))
                    {
                        // 记录短信发送成功日志
                        _logger.LogInformation("短信发送成功，手机号: {PhoneNumbers}, 模板代码: {TemplateCode}", phoneNumbers, templateCode);
                        return true;
                    }
                    // 记录短信发送失败但请求成功的日志
                    _logger.LogWarning("短信发送失败，响应内容: {ResponseContent}", responseContent);
                }
                else
                {
                    // 记录短信发送请求失败日志
                    _logger.LogError("短信发送请求失败，状态码: {StatusCode}", response.StatusCode);
                }
            }
        }
        catch (Exception ex)
        {
            // 记录短信发送过程中出现异常的日志
            _logger.LogError(ex, "短信发送过程中发生异常，手机号: {PhoneNumbers}, 模板代码: {TemplateCode}", phoneNumbers, templateCode);
        }
        return false;
    }

    /// <summary>
    /// 计算请求参数的签名
    /// </summary>
    /// <param name="parameters">请求参数</param>
    /// <param name="accessKeySecret">阿里云访问密钥 Secret</param>
    /// <returns>计算得到的签名</returns>
    private static string ComputeSignature(IDictionary<string, string> parameters, string accessKeySecret)
    {
        // 对请求参数按 Key 进行字典序排序
        var sortedParameters = parameters.OrderBy(p => p.Key);
        // 拼接排序后的参数
        var canonicalizedQueryString = string.Join("&", sortedParameters.Select(p => $"{UrlEncode(p.Key)}={UrlEncode(p.Value)}"));
        // 构建待签名的字符串
        var stringToSign = $"GET&{UrlEncode("/")}&{UrlEncode(canonicalizedQueryString)}";

        // 使用 HMAC-SHA1 算法计算签名
        using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes($"{accessKeySecret}&")))
        {
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            return Convert.ToBase64String(hashBytes);
        }
    }

    /// <summary>
    /// 对字符串进行 URL 编码
    /// </summary>
    /// <param name="value">待编码的字符串</param>
    /// <returns>编码后的字符串</returns>
    private static string UrlEncode(string value)
    {
        return System.Web.HttpUtility.UrlEncode(value, Encoding.UTF8)
            .Replace("+", "%20")
            .Replace("*", "%2A")
            .Replace("%7E", "~");
    }
}

public class AliyunSmsSettings
{
    public string AccessKeyId { get; set; }
    public string AccessKeySecret { get; set; }
    public string SignName { get; set; }
}