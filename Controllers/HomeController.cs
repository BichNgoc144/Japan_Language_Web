using System.Security.Claims;
using LearnJP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Controllers;

/// <summary>
/// Trang chủ (landing page) — giới thiệu sản phẩm, không yêu cầu đăng nhập.
/// Nếu user đã đăng nhập VÀ có to-do sắp/đã đến hạn có tick "Nhắc nhở", hiển thị thêm banner
/// nhắc nhở (AC: "chỉ có tick nhắc nhở mới có thông báo về trang chủ").
/// </summary>
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Nhắc trước: quá hạn + đến hạn hôm nay + trong 2 ngày tới, chưa hoàn thành,
            // VÀ đã tick "Nhắc nhở tôi ở trang chủ" — không tick thì không hiện.
            var reminders = await _db.TodoItems
                .Where(t => t.UserId == userId && !t.IsCompleted && t.EnableReminder && t.DueDate <= today.AddDays(2))
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new TodoReminderDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    IsOverdue = t.DueDate < today,
                    IsToday = t.DueDate == today
                })
                .ToListAsync();

            ViewData["TodoReminders"] = reminders;
        }

        return View();
    }
}