using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

public enum TodoPriority
{
    Low,
    Medium,
    High
}

/// <summary>Bảng MỚI — "todo_items". Việc cần làm của user, gắn theo 1 ngày cụ thể (hiển thị dạng lịch bảng).</summary>
[Table("todo_items")]
public class TodoItem
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required, MaxLength(200)]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("Note")]
    public string? Note { get; set; }

    [Column("DueDate")]
    public DateOnly DueDate { get; set; }

    [Column("IsCompleted")]
    public bool IsCompleted { get; set; } = false;

    /// <summary>Độ ưu tiên — ảnh hưởng cách hiển thị (màu) trong lịch, không ảnh hưởng logic khác.</summary>
    [Column("Priority")]
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    /// <summary>Chỉ khi tick "Nhắc nhở" mới xuất hiện ở banner thông báo trang chủ.</summary>
    [Column("EnableReminder")]
    public bool EnableReminder { get; set; } = false;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }
}