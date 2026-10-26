using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>
/// Bảng MỚI — "history_vocab". Ghi nhận số lần user trả lời ĐÚNG mỗi từ vựng (trong bài Kiểm tra),
/// bất kể chế độ trắc nghiệm nghĩa hay nghe-viết. Khoá nghiệp vụ: (UserId, VocaLevel, WordId)
/// vì WordId (vd "1-3") có thể trùng giữa bảng N5 và N4 nên bắt buộc phải kèm VocaLevel.
/// </summary>
[Table("history_vocab")]
public class HistoryVocab
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Column("VocaLevel")]
    public VocaLevel VocaLevel { get; set; }

    /// <summary>Trỏ tới minnano_voca_n5.id hoặc minnano_voca_n4.id tuỳ VocaLevel.</summary>
    [Column("WordId")]
    [MaxLength(10)]
    public string WordId { get; set; } = string.Empty;

    [Column("CorrectCount")]
    public int CorrectCount { get; set; } = 0;

    [Column("LastCorrectAt")]
    public DateTime? LastCorrectAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }
}