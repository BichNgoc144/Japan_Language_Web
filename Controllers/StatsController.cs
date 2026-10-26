using System.Security.Claims;
using LearnJP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Controllers;

/// <summary>
/// Thống kê tiến độ học tập. Dùng bảng "history_vocab" — mỗi dòng đại diện 1 TỪ user đã từng
/// trả lời đúng ít nhất 1 lần (CreatedAt = lần đúng đầu tiên). Đây là số liệu chính xác ở mức
/// TỪNG TỪ, thay cho cách tính cũ suy ra từ lesson_progress (vốn chỉ biết ngày pass cả bài).
/// </summary>
[Authorize]
public class StatsController : Controller
{
    private readonly AppDbContext _db;
    private const int ChartDays = 30;

    public StatsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var userLearn = await _db.UserLearns.FirstOrDefaultAsync(l => l.UserId == userId);

        var learnedWords = await _db.HistoryVocabs
            .Where(h => h.UserId == userId)
            .Select(h => h.CreatedAt)
            .ToListAsync();

        var byDate = learnedWords
            .GroupBy(d => DateOnly.FromDateTime(d))
            .ToDictionary(g => g.Key, g => g.Count());

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var series = new List<DailyWordCountDto>();
        for (var d = today.AddDays(-(ChartDays - 1)); d <= today; d = d.AddDays(1))
        {
            series.Add(new DailyWordCountDto
            {
                Date = d.ToString("dd/MM"),
                Count = byDate.GetValueOrDefault(d, 0)
            });
        }

        var lessonsPassed = await _db.LessonProgresses.CountAsync(p => p.UserId == userId && p.IsPassed);

        var vm = new StatsViewModel
        {
            CurrentLevel = userLearn?.Level ?? 0,
            WordsToday = byDate.GetValueOrDefault(today, 0),
            WordsTotal = learnedWords.Count,
            LessonsPassed = lessonsPassed,
            DailySeries = series
        };

        return View(vm);
    }
}