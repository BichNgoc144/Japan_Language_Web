namespace LearnJP.Models;

/// <summary>Logic dùng chung giữa Controller và sidebar (partial view) cho tính năng học từ vựng theo bài.</summary>
public static class LessonHelper
{
    /// <summary>Tổng số bài mỗi N-level (đã xác nhận: 25 bài/N).</summary>
    public const int TotalLessons = 25;

    /// <summary>Ngưỡng % tối thiểu để coi là "qua bài" — cấu hình trong khoảng 80-100 theo yêu cầu.</summary>
    public const int PassThresholdPercent = 80;

    /// <summary>Tách số bài từ tiền tố id trước dấu "-" (vd "1-3" -> 1). Trả về 0 nếu không parse được.</summary>
    public static int ParseLessonNumber(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return 0;
        var prefix = id.Split('-')[0];
        return int.TryParse(prefix, out var n) ? n : 0;
    }

    /// <summary>Bài 1 luôn mở; bài N&gt;1 chỉ mở khi bài N-1 đã pass.</summary>
    public static bool IsLessonUnlocked(int lessonNumber, HashSet<int> passedLessons)
    {
        if (lessonNumber <= 1) return true;
        return passedLessons.Contains(lessonNumber - 1);
    }

    /// <summary>N4 chỉ mở khi user đã qua hết 25 bài N5 (UserLearn.Level &gt;= 1).</summary>
    public static bool IsN4Unlocked(int userLevel) => userLevel >= 1;
}