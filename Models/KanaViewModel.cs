namespace LearnJP.Models;

/// <summary>1 ô ký tự trong bảng chữ cái — dùng cho cả hiển thị bảng và dữ liệu JS làm bài kiểm tra.</summary>
public class KanaItemDto
{
    public string Character { get; set; } = string.Empty;
    public string Romaji { get; set; } = string.Empty;
}

public class KanaIndexViewModel
{
    public List<KanaItemDto> HiraganaList { get; set; } = new();
    public List<KanaItemDto> KatakanaList { get; set; } = new();
}