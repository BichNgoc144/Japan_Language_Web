using System.Security.Claims;
using LearnJP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Controllers;

/// <summary>
/// Ôn tập — luyện lại các từ đang có trong "wrong_vocab" (sổ tay lỗi sai), ưu tiên từ sai nhiều
/// lần nhất / lâu nhất chưa sửa lên trước. Trả lời ĐÚNG khi ôn tập -> xoá khỏi wrong_vocab (coi
/// như đã khắc phục) + vẫn cộng vào history_vocab như bình thường. Trả lời SAI -> tăng WrongCount,
/// từ đó tiếp tục nằm trong sổ tay lỗi sai để lần ôn tập sau ưu tiên lại.
/// </summary>
[Authorize]
public class ReviewController : Controller
{
    private readonly AppDbContext _db;

    public ReviewController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();

        var wrongList = await _db.WrongVocabs
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.WrongCount)
            .ThenBy(w => w.LastWrongAt)
            .ToListAsync();

        var n5Ids = wrongList.Where(w => w.VocaLevel == VocaLevel.N5).Select(w => w.WordId).ToHashSet();
        var n4Ids = wrongList.Where(w => w.VocaLevel == VocaLevel.N4).Select(w => w.WordId).ToHashSet();

        var n5Map = (await _db.MinnanoVocaN5s.Where(w => n5Ids.Contains(w.Id)).ToListAsync())
            .ToDictionary(w => w.Id);
        var n4Map = (await _db.MinnanoVocaN4s.Where(w => n4Ids.Contains(w.Id)).ToListAsync())
            .ToDictionary(w => w.Id);

        var candidates = new List<ReviewWordDto>();
        foreach (var w in wrongList)
        {
            if (w.VocaLevel == VocaLevel.N5 && n5Map.TryGetValue(w.WordId, out var w5))
            {
                candidates.Add(new ReviewWordDto
                {
                    VocaLevel = "N5",
                    WordId = w.WordId,
                    Kanji = w5.Kanji,
                    HiraKana = w5.HiraKana,
                    MeaningVn = w5.MeaningVn,
                    WrongCount = w.WrongCount
                });
            }
            else if (w.VocaLevel == VocaLevel.N4 && n4Map.TryGetValue(w.WordId, out var w4))
            {
                candidates.Add(new ReviewWordDto
                {
                    VocaLevel = "N4",
                    WordId = w.WordId,
                    Kanji = w4.Kanji,
                    HiraKana = w4.HiraKana,
                    MeaningVn = w4.MeaningVn,
                    WrongCount = w.WrongCount
                });
            }
        }

        return View(new ReviewIndexViewModel { Candidates = candidates });
    }

    /// <summary>
    /// Đúng -> xoá khỏi wrong_vocab + cộng history_vocab. Sai -> tăng WrongCount trong wrong_vocab
    /// (giữ nguyên trong sổ tay để lần ôn tập sau ưu tiên lại).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReviewResult([FromBody] SubmitReviewResultDto dto)
    {
        if (dto.Answers is null || dto.Answers.Count == 0) return BadRequest();

        var userId = GetCurrentUserId();
        var correctAnswers = dto.Answers.Where(a => a.IsCorrect).ToList();
        var wrongAnswers = dto.Answers.Where(a => !a.IsCorrect).ToList();

        // ---------- Đúng: xoá khỏi wrong_vocab + cộng history_vocab ----------
        foreach (var group in correctAnswers.GroupBy(a => (a.VocaLevel, a.WordId)))
        {
            if (!Enum.TryParse<VocaLevel>(group.Key.VocaLevel, true, out var level)) continue;

            var wrong = await _db.WrongVocabs.FirstOrDefaultAsync(
                w => w.UserId == userId && w.VocaLevel == level && w.WordId == group.Key.WordId);
            if (wrong is not null) _db.WrongVocabs.Remove(wrong);

            var history = await _db.HistoryVocabs.FirstOrDefaultAsync(
                h => h.UserId == userId && h.VocaLevel == level && h.WordId == group.Key.WordId);
            if (history is not null)
            {
                history.CorrectCount++;
                history.LastCorrectAt = DateTime.UtcNow;
                history.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.HistoryVocabs.Add(new HistoryVocab
                {
                    UserId = userId,
                    VocaLevel = level,
                    WordId = group.Key.WordId,
                    CorrectCount = 1,
                    LastCorrectAt = DateTime.UtcNow
                });
            }
        }

        // ---------- Sai: tăng WrongCount, tiếp tục nằm trong sổ tay lỗi sai ----------
        foreach (var group in wrongAnswers.GroupBy(a => (a.VocaLevel, a.WordId)))
        {
            if (!Enum.TryParse<VocaLevel>(group.Key.VocaLevel, true, out var level)) continue;

            var wrong = await _db.WrongVocabs.FirstOrDefaultAsync(
                w => w.UserId == userId && w.VocaLevel == level && w.WordId == group.Key.WordId);
            if (wrong is not null)
            {
                wrong.WrongCount++;
                wrong.LastWrongAt = DateTime.UtcNow;
            }
            else
            {
                _db.WrongVocabs.Add(new WrongVocab
                {
                    UserId = userId,
                    VocaLevel = level,
                    WordId = group.Key.WordId,
                    WrongCount = 1,
                    LastWrongAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();

        var total = dto.Answers.Count;
        var correct = correctAnswers.Count;

        return Json(new SubmitReviewResultResponseDto
        {
            CorrectCount = correct,
            TotalCount = total,
            Message = correct == total
                ? $"Xuất sắc! Bạn đã sửa hết {correct} từ sai — đã xoá khỏi sổ tay lỗi sai."
                : $"Bạn ôn đúng {correct}/{total} từ. {total - correct} từ còn lại vẫn nằm trong sổ tay lỗi sai để ôn tiếp."
        });
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}