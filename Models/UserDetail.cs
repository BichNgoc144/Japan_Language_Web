using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

public enum Gender
{
    Male,
    Female,
    Other
}

/// <summary>
/// Ánh xạ bảng "userdetails" đã có sẵn trong DB — quan hệ 1-1 với "users"
/// (UserId vừa là PK vừa là FK, không có PK riêng).
/// Cột gốc: UserId, FullName, Avatar, Gender, DateOfBirth, Phone, Address, University, Major, Bio.
/// Cột bổ sung cho epic Đăng nhập/Đăng ký (đã duyệt): AvatarPublicId (dọn ảnh cũ trên Cloudinary).
/// </summary>
[Table("userdetails")]
public class UserDetail
{
    /// <summary>PK đồng thời là FK trỏ tới users.UserId (cấu hình 1-1 ở DbContext).</summary>
    [Key]
    [Column("UserId")]
    public int UserId { get; set; }

    [MaxLength(100)]
    [Column("FullName")]
    public string? FullName { get; set; }

    /// <summary>URL ảnh đại diện lưu trên Cloudinary. Null = chưa upload, FE hiển thị avatar mặc định.</summary>
    [MaxLength(255)]
    [Column("Avatar")]
    public string? Avatar { get; set; }

    [Column("Gender")]
    public Gender? Gender { get; set; }

    [Column("DateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    [Column("Phone")]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [Column("Address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Column("University")]
    public string? University { get; set; }

    [MaxLength(100)]
    [Column("Major")]
    public string? Major { get; set; }

    [Column("Bio")]
    public string? Bio { get; set; }

    /// <summary>Bổ sung: public_id của ảnh trên Cloudinary, cần để xoá ảnh cũ khi user đổi avatar mới.</summary>
    [MaxLength(255)]
    [Column("AvatarPublicId")]
    public string? AvatarPublicId { get; set; }

    // ----- Navigation property (quan hệ 1-1 ngược về users) -----
    public User? User { get; set; }
}
