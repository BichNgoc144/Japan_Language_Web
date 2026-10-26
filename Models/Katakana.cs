using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>
/// Ánh xạ bảng "katakana" có sẵn — dữ liệu tham chiếu, chỉ đọc. Không thay đổi cấu trúc.
/// Lưu ý: tên cột gốc viết thường (katakana/romaji), khác quy ước PascalCase của bảng "hiragana".
/// </summary>
[Table("katakana")]
public class Katakana
{
    [Key]
    [Column("katakana")]
    [MaxLength(10)]
    public string Character { get; set; } = string.Empty;

    [Column("romaji")]
    [MaxLength(10)]
    public string Romaji { get; set; } = string.Empty;
}