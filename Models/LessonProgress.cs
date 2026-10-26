using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

public enum VocaLevel
{
    N5,
    N4
}

/// <summary>
/// Bảng MỚI (đã duyệt) — "lesson_progress". Lưu tiến độ học từ vựng theo từng bài (1-25) của mỗi N-level.
/// Quy tắc mở khoá: Bài 1 luôn mở; Bài N (N&gt;1) chỉ mở khi Bài N-1 đã IsPassed=true.
/// N4 (toàn bộ 25 bài) chỉ mở khi User.UserLearn.Level &gt;= 1 (tức đã qua hết 25 bài N5).
/// </summary>
[Table("lesson_progress")]
public class LessonProgress
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Column("VocaLevel")]
    public VocaLevel VocaLevel { get; set; }

    [Column("LessonNumber")]
    public int LessonNumber { get; set; }

    [Column("IsPassed")]
    public bool IsPassed { get; set; } = false;

    /// <summary>Điểm % cao nhất từng đạt được ở bài này (giữ lại thành tích tốt nhất qua nhiều lần làm).</summary>
    [Column("BestScore")]
    public int? BestScore { get; set; }

    [Column("Attempts")]
    public int Attempts { get; set; } = 0;

    [Column("CompletedAt")]
    public DateTime? CompletedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }
}