using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public class PBKDF2Encrypt
{
    /// <summary>
    /// 对输入的密码进行 PBKDF2 哈希处理
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <returns>包含盐和哈希值的字符串，以冒号分隔</returns>
    public static string HashPassword(string password)
    {
        byte[] salt = new byte[128 / 8];
        // 使用随机数生成器生成盐值
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return $"{Convert.ToBase64String(salt)}:{hashed}";
    }

    /// <summary>
    /// 验证输入的密码与存储的 PBKDF2 哈希密码是否匹配
    /// </summary>
    /// <param name="password">用户输入的原始密码</param>
    /// <param name="hashedPassword">存储的哈希密码</param>
    /// <returns>如果匹配返回 true，否则返回 false</returns>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        // 检查输入是否包含冒号
        if (string.IsNullOrEmpty(hashedPassword) || !hashedPassword.Contains(':'))
        {
            return false;
        }

        string[] parts = hashedPassword.Split(':');
        // 检查分割后的数组长度是否符合预期
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[0]);
            string expectedHash = parts[1];

            string actualHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return actualHash == expectedHash;
        }
        catch (FormatException)
        {
            // 处理 Base64 转换失败的情况
            return false;
        }
    }

    /// <summary>
    /// 判断字符串是否为 PBKDF2 格式（盐值Base64编码:派生密钥Base64编码 格式）
    /// </summary>
    /// <param name="input">待判断的字符串</param>
    /// <returns>如果是 PBKDF2 格式返回 true，否则返回 false</returns>
    public static bool IsPbkdf2(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        // 正则表达式匹配 盐值Base64编码:派生密钥Base64编码 格式
        var pattern = @"^[A-Za-z0-9+/=]+:[A-Za-z0-9+/=]+$";
        return Regex.IsMatch(input, pattern);
    }
}