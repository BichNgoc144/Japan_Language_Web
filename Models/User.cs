using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

public enum UserRole
{
    Student,
    Admin
}

/// <summary>
/// Ánh xạ bảng "users" đã có sẵn trong DB (schema japan_language).
/// Cột gốc: UserId, Username, Email, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt.
/// Cột bổ sung cho epic Đăng nhập/Đăng ký (đã duyệt): FailedLoginAttempts, LockoutEndAt, SecurityStamp.
/// </summary>
[Table("users")]
public class User
{
    [Key]
    [Column("UserId")]
    public int UserId { get; set; }

    [Required, MaxLength(50)]
    [Column("Username")]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Lưu dưới dạng chuỗi khớp enum MySQL 'Student'/'Admin' (cấu hình HasConversion ở DbContext).</summary>
    [Column("Role")]
    public UserRole Role { get; set; } = UserRole.Student;

    /// <summary>
    /// true = tài khoản hoạt động bình thường. false = bị Admin khoá (không dùng cột riêng IsBanned).
    /// Vì OTP được xác thực TRƯỚC khi dòng users được tạo, IsActive không mang nghĩa "email chưa xác thực".
    /// </summary>
    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    // ----- Bổ sung: chống brute-force đăng nhập (khoá 15 phút sau 5 lần sai) -----
    [Column("FailedLoginAttempts")]
    public int FailedLoginAttempts { get; set; } = 0;

    [Column("LockoutEndAt")]
    public DateTime? LockoutEndAt { get; set; }

    /// <summary>
    /// Bổ sung: dùng với Cookie Authentication để vô hiệu hoá toàn bộ cookie đã phát hành
    /// (đổi mật khẩu / reset mật khẩu / đăng xuất mọi thiết bị) mà không cần bảng session riêng.
    /// Đổi giá trị này = mọi cookie cũ (có mang SecurityStamp khác) sẽ bị coi là hết hạn.
    /// </summary>
    [MaxLength(50)]
    [Column("SecurityStamp")]
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    // ----- Navigation properties (quan hệ 1-1) -----
    public UserDetail? UserDetail { get; set; }
    public UserLearn? UserLearn { get; set; }
}
