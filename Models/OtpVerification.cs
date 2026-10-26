using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

public enum OtpPurpose
{
    Register,
    ResetPassword
}

/// <summary>
/// Bảng MỚI (đã duyệt) — "otp_verifications". Lưu mã OTP đã băm cho 1 email + 1 mục đích cụ thể.
/// Dùng chung cho luồng Đăng ký (xác thực email trước khi tạo mật khẩu) và Quên mật khẩu.
/// AC: OTP 6 số, hiệu lực 5 phút, sai quá 3 lần phải yêu cầu gửi lại OTP mới.
/// </summary>
[Table("otp_verifications")]
public class OtpVerification
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Required, MaxLength(255)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Mã OTP đã băm (BCrypt) — không lưu plain text.</summary>
    [Required, MaxLength(255)]
    [Column("OtpCodeHash")]
    public string OtpCodeHash { get; set; } = string.Empty;

    [Column("Purpose")]
    public OtpPurpose Purpose { get; set; }

    [Column("AttemptCount")]
    public int AttemptCount { get; set; } = 0;

    [NotMapped]
    public const int MaxAttempts = 3;

    [Column("IsVerified")]
    public bool IsVerified { get; set; } = false;

    [Column("IsUsed")]
    public bool IsUsed { get; set; } = false;

    [Column("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
