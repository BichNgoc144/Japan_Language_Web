namespace LearnJP.Models;

/// <summary>1 từ vựng — dùng chung cho hiển thị Học và dữ liệu JS làm bài Kiểm tra.</summary>
public class VocaWordDto
{
    public string Id { get; set; } = string.Empty;
    public string? Kanji { get; set; }
    public string? HiraKana { get; set; }
    public string? MeaningVn { get; set; }
    public string? MeaningEn { get; set; }
}

/// <summary>Trang "Học" — danh sách từ vựng của 1 bài.</summary>
public class LessonStudyViewModel
{
    public VocaLevel VocaLevel { get; set; }
    public int LessonNumber { get; set; }
    public List<VocaWordDto> Words { get; set; } = new();
}

/// <summary>Trang "Kiểm tra" — dữ liệu bài + trạng thái đã qua bài này chưa (hiển thị điểm cũ nếu có).</summary>
public class LessonQuizViewModel
{
    public VocaLevel VocaLevel { get; set; }
    public int LessonNumber { get; set; }
    public List<VocaWordDto> Words { get; set; } = new();
    public bool AlreadyPassed { get; set; }
    public int? BestScore { get; set; }

    /// <summary>Nghĩa của TOÀN BỘ từ trong level (N5/N4) — dùng làm đáp án nhiễu cho trắc nghiệm 1/4.</summary>
    public List<string> MeaningPool { get; set; } = new();
}

/// <summary>Kết quả chấm 1 từ trong bài kiểm tra — dùng để ghi nhận vào history_vocab khi đúng.</summary>
public class VocaAnswerResultDto
{
    public string WordId { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

/// <summary>Payload gửi lên khi nộp bài kiểm tra để server chấm và cập nhật lesson_progress + history_vocab.</summary>
public class SubmitQuizResultDto
{
    public string VocaLevel { get; set; } = string.Empty; // "N5" | "N4"
    public int LessonNumber { get; set; }
    public int ElapsedSeconds { get; set; }

    /// <summary>Kết quả TỪNG từ — server tự tính đúng/tổng từ danh sách này, không tin số client tổng hợp sẵn.</summary>
    public List<VocaAnswerResultDto> Answers { get; set; } = new();
}

/// <summary>Kết quả trả về sau khi nộp bài — FE dùng để hiển thị thông báo qua bài / lên Level.</summary>
public class SubmitQuizResultResponseDto
{
    public bool Passed { get; set; }
    public int PercentScore { get; set; }
    public bool LeveledUp { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>1 bài trong danh sách sổ xuống ở sidebar (kèm trạng thái khoá/qua).</summary>
public class LessonBoxDto
{
    public int LessonNumber { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsPassed { get; set; }
}