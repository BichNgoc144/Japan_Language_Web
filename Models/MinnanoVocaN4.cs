using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>Ánh xạ bảng "minnano_voca_n4" có sẵn — dữ liệu tham chiếu, chỉ đọc. Cùng quy ước id với N5.</summary>
[Table("minnano_voca_n4")]
public class MinnanoVocaN4
{
    [Key]
    [Column("id")]
    [MaxLength(10)]
    public string Id { get; set; } = string.Empty;

    [Column("kanji")]
    [MaxLength(100)]
    public string? Kanji { get; set; }

    [Column("hira_kana")]
    [MaxLength(100)]
    public string? HiraKana { get; set; }

    [Column("meaning_en")]
    [MaxLength(100)]
    public string? MeaningEn { get; set; }

    [Column("meaning_vn")]
    [MaxLength(100)]
    public string? MeaningVn { get; set; }

    [NotMapped]
    public int LessonNumber => LessonHelper.ParseLessonNumber(Id);
}