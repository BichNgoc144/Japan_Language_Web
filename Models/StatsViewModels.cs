namespace LearnJP.Models;

public class DailyWordCountDto
{
    public string Date { get; set; } = string.Empty; 
    public int Count { get; set; }
}

public class StatsViewModel
{
    public int CurrentLevel { get; set; }
    public int WordsToday { get; set; }
    public int WordsTotal { get; set; }
    public int LessonsPassed { get; set; }
    public List<DailyWordCountDto> DailySeries { get; set; } = new();
}