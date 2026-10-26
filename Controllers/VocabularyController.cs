using System.Security.Claims;
using LearnJP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Controllers;

[Authorize]
public class VocabularyController : Controller
{
    private readonly AppDbContext _db;

    public VocabularyController(AppDbContext db)
    {
        _db = db;
    }

    // ================= HỌC =================

    [HttpGet]
    public async Task<IActionResult> Study(string level, int number)
    {
        if (!TryParseLevel(level, out var vocaLevel)) return NotFound();

        var access = await CheckAccessAsync(vocaLevel, number);
        if (!access.ok)
        {
            TempData["Error"] = access.message;
            return RedirectToAction("Index", "Home");
        }

        var words = await GetLessonWordsAsync(vocaLevel, number);
        return View(new LessonStudyViewModel
        {
            VocaLevel = vocaLevel,
            LessonNumber = number,
            Words = words
        });
    }

    // ================= KIỂM TRA =================

    [HttpGet]
    public async Task<IActionResult> Quiz(string level, int number)
    {
        if (!TryParseLevel(level, out var vocaLevel)) return NotFound();

        var access = await CheckAccessAsync(vocaLevel, number);
        if (!access.ok)
        {
            TempData["Error"] = access.message;
            return RedirectToAction("Index", "Home");
        }

        var words = await GetLessonWordsAsync(vocaLevel, number);
        var userId = GetCurrentUserId();
        var progress = await _db.LessonProgresses.FirstOrDefaultAsync(
            p => p.UserId == userId && p.VocaLevel == vocaLevel && p.LessonNumber == number);

        var meaningPool = await GetMeaningPoolAsync(vocaLevel);

        return View(new LessonQuizViewModel
        {
            VocaLevel = vocaLevel,
            LessonNumber = number,
            Words = words,
            AlreadyPassed = progress?.IsPassed ?? false,
            BestScore = progress?.BestScore,
            MeaningPool = meaningPool
        });
    }

    /// <summary>
    /// Chấm điểm THEO TỪNG TỪ (server tự tính, không tin số client gửi sẵn) + lưu tiến độ bài học.
    /// Nếu đạt ngưỡng (AC: 80-100%, mặc định 80%) -> IsPassed = true, mở khoá bài tiếp theo.
    /// Nếu vừa pass đủ 25/25 bài N5 -> tăng UserLearn.Level lên 1 (mở khoá N4).
    /// Mỗi từ trả lời ĐÚNG -> ghi/nâng số lần đúng vào history_vocab (dùng chung cho cả 2 chế độ
    /// trắc nghiệm nghĩa và nghe-viết).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuizResult([FromBody] SubmitQuizResultDto dto)
    {
        if (!TryParseLevel(dto.VocaLevel, out var vocaLevel) || dto.Answers is null || dto.Answers.Count == 0)
            return BadRequest();

        var access = await CheckAccessAsync(vocaLevel, dto.LessonNumber);
        if (!access.ok) return Forbid();

        var totalCount = dto.Answers.Count;
        var correctCount = dto.Answers.Count(a => a.IsCorrect);
        var pct = (int)Math.Round(correctCount * 100.0 / totalCount);
        var passed = pct >= LessonHelper.PassThresholdPercent;

        var userId = GetCurrentUserId();

        // ---------- Ghi history_vocab cho từng từ trả lời đúng ----------
        var correctWordIds = dto.Answers.Where(a => a.IsCorrect).Select(a => a.WordId).Distinct().ToList();
        if (correctWordIds.Count > 0)
        {
            var existingHistory = await _db.HistoryVocabs
                .Where(h => h.UserId == userId && h.VocaLevel == vocaLevel && correctWordIds.Contains(h.WordId))
                .ToListAsync();
            var existingMap = existingHistory.ToDictionary(h => h.WordId);

            foreach (var wordId in correctWordIds)
            {
                if (existingMap.TryGetValue(wordId, out var history))
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
                        VocaLevel = vocaLevel,
                        WordId = wordId,
                        CorrectCount = 1,
                        LastCorrectAt = DateTime.UtcNow
                    });
                }
            }

            // Trả lời đúng -> từ đó không còn là "lỗi sai" nữa, gỡ khỏi sổ tay lỗi sai (nếu có).
            var wrongToRemove = await _db.WrongVocabs
                .Where(w => w.UserId == userId && w.VocaLevel == vocaLevel && correctWordIds.Contains(w.WordId))
                .ToListAsync();
            if (wrongToRemove.Count > 0) _db.WrongVocabs.RemoveRange(wrongToRemove);
        }

        // ---------- Ghi/tăng wrong_vocab cho từng từ trả lời sai (sổ tay lỗi sai, dùng cho mục Ôn tập) ----------
        var wrongWordIds = dto.Answers.Where(a => !a.IsCorrect).Select(a => a.WordId).Distinct().ToList();
        if (wrongWordIds.Count > 0)
        {
            var existingWrong = await _db.WrongVocabs
                .Where(w => w.UserId == userId && w.VocaLevel == vocaLevel && wrongWordIds.Contains(w.WordId))
                .ToListAsync();
            var existingWrongMap = existingWrong.ToDictionary(w => w.WordId);

            foreach (var wordId in wrongWordIds)
            {
                if (existingWrongMap.TryGetValue(wordId, out var wrong))
                {
                    wrong.WrongCount++;
                    wrong.LastWrongAt = DateTime.UtcNow;
                }
                else
                {
                    _db.WrongVocabs.Add(new WrongVocab
                    {
                        UserId = userId,
                        VocaLevel = vocaLevel,
                        WordId = wordId,
                        WrongCount = 1,
                        LastWrongAt = DateTime.UtcNow
                    });
                }
            }
        }

        // ---------- Cập nhật lesson_progress ----------
        var progress = await _db.LessonProgresses.FirstOrDefaultAsync(
            p => p.UserId == userId && p.VocaLevel == vocaLevel && p.LessonNumber == dto.LessonNumber);

        if (progress is null)
        {
            progress = new LessonProgress
            {
                UserId = userId,
                VocaLevel = vocaLevel,
                LessonNumber = dto.LessonNumber
            };
            _db.LessonProgresses.Add(progress);
        }

        progress.Attempts++;
        progress.BestScore = Math.Max(progress.BestScore ?? 0, pct);
        progress.UpdatedAt = DateTime.UtcNow;
        if (passed && !progress.IsPassed)
        {
            progress.IsPassed = true;
            progress.CompletedAt = DateTime.UtcNow;
        }

        var leveledUp = false;

        // AC: qua hết 25 bài N5 -> Level lên 1, mở khoá N4.
        if (passed && vocaLevel == VocaLevel.N5)
        {
            await _db.SaveChangesAsync(); // đảm bảo progress.IsPassed vừa đổi đã có trong DB trước khi đếm

            var passedN5Count = await _db.LessonProgresses
                .CountAsync(p => p.UserId == userId && p.VocaLevel == VocaLevel.N5 && p.IsPassed);

            if (passedN5Count >= LessonHelper.TotalLessons)
            {
                var userLearn = await _db.UserLearns.FirstOrDefaultAsync(l => l.UserId == userId);
                if (userLearn is not null && userLearn.Level < 1)
                {
                    userLearn.Level = 1;
                    userLearn.Time = DateTime.UtcNow;
                    leveledUp = true;
                }
            }
        }

        await _db.SaveChangesAsync();

        var message = !passed
            ? $"Bạn đạt {pct}%, cần tối thiểu {LessonHelper.PassThresholdPercent}% để qua bài. Hãy luyện thêm!"
            : leveledUp
                ? "Chúc mừng! Bạn đã hoàn thành hết N5 và lên Level 1 — N4 đã được mở khoá."
                : $"Qua bài! Đạt {pct}%. Bài tiếp theo đã được mở khoá.";

        return Json(new SubmitQuizResultResponseDto
        {
            Passed = passed,
            PercentScore = pct,
            LeveledUp = leveledUp,
            Message = message
        });
    }

    // ================= Helper dùng nội bộ Controller =================

    private static bool TryParseLevel(string? level, out VocaLevel vocaLevel) =>
        Enum.TryParse(level, true, out vocaLevel);

    private async Task<List<VocaWordDto>> GetLessonWordsAsync(VocaLevel level, int lessonNumber)
    {
        if (level == VocaLevel.N5)
        {
            var all = await _db.MinnanoVocaN5s.ToListAsync();
            return all.Where(w => w.LessonNumber == lessonNumber)
                       .OrderBy(w => w.Id)
                       .Select(w => new VocaWordDto { Id = w.Id, Kanji = w.Kanji, HiraKana = w.HiraKana, MeaningVn = w.MeaningVn, MeaningEn = w.MeaningEn })
                       .ToList();
        }
        else
        {
            var all = await _db.MinnanoVocaN4s.ToListAsync();
            return all.Where(w => w.LessonNumber == lessonNumber)
                       .OrderBy(w => w.Id)
                       .Select(w => new VocaWordDto { Id = w.Id, Kanji = w.Kanji, HiraKana = w.HiraKana, MeaningVn = w.MeaningVn, MeaningEn = w.MeaningEn })
                       .ToList();
        }
    }

    /// <summary>Lấy toàn bộ nghĩa (distinct, không rỗng) của cả level — dùng làm đáp án nhiễu cho trắc nghiệm 1/4.</summary>
    private async Task<List<string>> GetMeaningPoolAsync(VocaLevel level)
    {
        IQueryable<string?> query = level == VocaLevel.N5
            ? _db.MinnanoVocaN5s.Select(w => w.MeaningVn)
            : _db.MinnanoVocaN4s.Select(w => w.MeaningVn);

        var meanings = await query.ToListAsync();
        return meanings.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().Select(m => m!).ToList();
    }

    /// <summary>Kiểm tra quyền truy cập bài học: N4 cần Level>=1; bài N&gt;1 cần bài N-1 đã pass.</summary>
    private async Task<(bool ok, string message)> CheckAccessAsync(VocaLevel level, int lessonNumber)
    {
        if (lessonNumber < 1 || lessonNumber > LessonHelper.TotalLessons)
            return (false, "Bài học không tồn tại.");

        var userId = GetCurrentUserId();

        if (level == VocaLevel.N4)
        {
            var userLearn = await _db.UserLearns.FirstOrDefaultAsync(l => l.UserId == userId);
            if (!LessonHelper.IsN4Unlocked(userLearn?.Level ?? 0))
                return (false, "Bạn cần hoàn thành hết 25 bài N5 trước khi học N4.");
        }

        if (lessonNumber > 1)
        {
            var passedLessons = await _db.LessonProgresses
                .Where(p => p.UserId == userId && p.VocaLevel == level && p.IsPassed)
                .Select(p => p.LessonNumber)
                .ToListAsync();

            if (!LessonHelper.IsLessonUnlocked(lessonNumber, passedLessons.ToHashSet()))
                return (false, $"Bạn cần hoàn thành Bài {lessonNumber - 1} trước khi học Bài {lessonNumber}.");
        }

        return (true, string.Empty);
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}