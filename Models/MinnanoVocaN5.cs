using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearnJP.Models;

/// <summary>
/// Ánh xạ bảng "minnano_voca_n5" có sẵn — dữ liệu tham chiếu, chỉ đọc.
/// Quy ước đã xác nhận: tiền tố `id` trước dấu "-" là số bài (vd "1-3" = bài 1, từ thứ 3).
/// Tổng 25 bài/N-level.
/// </summary>
[Table("minnano_voca_n5")]
public class MinnanoVocaN5
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
    public string? MeaningEn { get; set; }

    [Column("meaning_vn")]
    public string? MeaningVn { get; set; }

    /// <summary>Số bài, tách từ tiền tố id trước dấu "-". Không map cột DB (tính toán).</summary>
    [NotMapped]
    public int LessonNumber => LessonHelper.ParseLessonNumber(Id);
}