namespace LearnJP.Models;

/// <summary>1 từ trong sổ tay lỗi sai — kèm VocaLevel vì WordId có thể trùng giữa N5/N4.</summary>
public class ReviewWordDto
{
    public string VocaLevel { get; set; } = string.Empty; // "N5" | "N4"
    public string WordId { get; set; } = string.Empty;
    public string? Kanji { get; set; }
    public string? HiraKana { get; set; }
    public string? MeaningVn { get; set; }
    public int WrongCount { get; set; }
}

/// <summary>Toàn bộ sổ tay lỗi sai (wrong_vocab), sắp xếp sai nhiều nhất/lâu nhất lên trước.</summary>
public class ReviewIndexViewModel
{
    public List<ReviewWordDto> Candidates { get; set; } = new();
    public int TotalCount => Candidates.Count;
}

public class ReviewAnswerDto
{
    public string VocaLevel { get; set; } = string.Empty;
    public string WordId { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class SubmitReviewResultDto
{
    public List<ReviewAnswerDto> Answers { get; set; } = new();
}

public class SubmitReviewResultResponseDto
{
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public string Message { get; set; } = string.Empty;
}