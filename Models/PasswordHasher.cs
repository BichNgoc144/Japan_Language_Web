using System.Security.Cryptography;

namespace LearnJP.Models;

/// <summary>
/// Băm mật khẩu bằng PBKDF2 (Rfc2898DeriveBytes) — có sẵn trong .NET, không cần cài thêm
/// package ngoài (BCrypt.Net...). Định dạng lưu: "{iterations}.{saltBase64}.{hashBase64}".
/// Dùng chung cho mật khẩu tài khoản VÀ mã OTP (không lưu OTP dạng plain text).
/// </summary>
public static class PasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;   // 128-bit
    private const int HashSize = 32;   // 256-bit

    public static string Hash(string plainText)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(plainText, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string plainText, string hashedValue)
    {
        var parts = hashedValue.Split('.', 3);
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out var iterations)) return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(plainText, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    /// <summary>Băm 1 chiều (SHA-256, không cần salt) — dùng cho token quên mật khẩu (so khớp chính xác).</summary>
    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    public static string GenerateRawToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    }

    public static string GenerateOtpCode()
    {
        return RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    }

    /// <summary>Sinh mật khẩu mạnh ngẫu nhiên ≥12 ký tự, đủ hoa/thường/số/ký tự đặc biệt.</summary>
    public static string GenerateStrongPassword(int length = 12)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%^&*()-_=+?";
        const string all = upper + lower + digits + special;

        if (length < 12) length = 12;
        var chars = new char[length];
        chars[0] = upper[RandomNumberGenerator.GetInt32(0, upper.Length)];
        chars[1] = lower[RandomNumberGenerator.GetInt32(0, lower.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(0, digits.Length)];
        chars[3] = special[RandomNumberGenerator.GetInt32(0, special.Length)];
        for (int i = 4; i < length; i++)
            chars[i] = all[RandomNumberGenerator.GetInt32(0, all.Length)];

        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(0, i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars);
    }
}
