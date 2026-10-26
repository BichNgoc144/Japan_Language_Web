using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>
/// Bảng MỚI (đã duyệt) — "password_reset_tokens". Token dùng 1 lần cho luồng "Quên mật khẩu".
/// AC: token ngẫu nhiên (được băm trước khi lưu), hết hạn sau 15 phút, chỉ dùng được 1 lần.
/// </summary>
[Table("password_reset_tokens")]
public class PasswordResetToken
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [ForeignKey(nameof(User))]
    [Column("UserId")]
    public int UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Token được băm (SHA-256) trước khi lưu DB; token gốc chỉ gửi qua email, không lưu lại.</summary>
    [Required, MaxLength(255)]
    [Column("TokenHash")]
    public string TokenHash { get; set; } = string.Empty;

    [Column("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    [Column("IsUsed")]
    public bool IsUsed { get; set; } = false;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
