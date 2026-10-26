using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>
/// Ánh xạ bảng "user_learn" đã có sẵn trong DB.
/// Theo xác nhận: mỗi user có đúng 1 dòng lưu tiến độ hiện tại (level, point) —
/// dòng này được tạo với level=0, point=0 ngay khi đăng ký thành công, sau đó module
/// Học tập sẽ update đè (không phải log lịch sử nhiều dòng).
/// Cột gốc: id, user_id, level, point, time. Không có cột mới bổ sung ở bảng này.
/// </summary>
[Table("user_learn")]
public class UserLearn
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Cấp độ học tập. Mặc định 0 khi mới đăng ký, tăng dần do module Học tập cập nhật.</summary>
    [Column("level")]
    public int Level { get; set; } = 0;

    /// <summary>Điểm tích luỹ. Mặc định 0 khi mới đăng ký.</summary>
    [Column("point")]
    public int Point { get; set; } = 0;

    /// <summary>Thời điểm cập nhật tiến độ gần nhất.</summary>
    [Column("time")]
    public DateTime Time { get; set; } = DateTime.UtcNow;

    // ----- Navigation property (quan hệ 1-1 ngược về users) -----
    public User? User { get; set; }
}
