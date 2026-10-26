using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>Ánh xạ bảng "hiragana" có sẵn — dữ liệu tham chiếu, chỉ đọc. Không thay đổi cấu trúc.</summary>
[Table("hiragana")]
public class Hiragana
{
    [Key]
    [Column("Hiragana")]
    [MaxLength(10)]
    public string Character { get; set; } = string.Empty;

    [Column("Romaji")]
    [MaxLength(10)]
    public string Romaji { get; set; } = string.Empty;
}