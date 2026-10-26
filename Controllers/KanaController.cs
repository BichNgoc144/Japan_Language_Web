using LearnJP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Controllers;

/// <summary>
/// Học Hiragana &amp; Katakana. Chỉ yêu cầu đăng nhập, KHÔNG ràng buộc theo Level của user
/// (khác với các module học từ vựng theo cấp độ sau này).
/// Bài kiểm tra chạy hoàn toàn phía client (JS) — không lưu kết quả vào DB theo yêu cầu.
/// </summary>
[Authorize]
public class KanaController : Controller
{
    private readonly AppDbContext _db;

    public KanaController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var hiragana = await _db.Hiraganas
            .OrderBy(h => h.Character)
            .Select(h => new KanaItemDto { Character = h.Character, Romaji = h.Romaji })
            .ToListAsync();

        var katakana = await _db.Katakanas
            .OrderBy(k => k.Character)
            .Select(k => new KanaItemDto { Character = k.Character, Romaji = k.Romaji })
            .ToListAsync();

        return View(new KanaIndexViewModel
        {
            HiraganaList = hiragana,
            KatakanaList = katakana
        });
    }
}