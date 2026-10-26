using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>
/// Bảng MỚI — "wrong_vocab" (sổ tay lỗi sai). Mỗi dòng là 1 từ user đang trả lời SAI, dùng làm
/// nguồn dữ liệu cho mục Ôn tập. Khi ôn tập lại mà trả lời ĐÚNG -> xoá khỏi bảng này (coi như đã
/// khắc phục). Khoá nghiệp vụ (UserId, VocaLevel, WordId) giống history_vocab.
/// </summary>
[Table("wrong_vocab")]
public class WrongVocab
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Column("VocaLevel")]
    public VocaLevel VocaLevel { get; set; }

    [Column("WordId")]
    [MaxLength(10)]
    public string WordId { get; set; } = string.Empty;

    [Column("WrongCount")]
    public int WrongCount { get; set; } = 0;

    [Column("LastWrongAt")]
    public DateTime LastWrongAt { get; set; } = DateTime.UtcNow;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}