using System.Globalization;
using System.Security.Claims;
using LearnJP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Controllers;

[Authorize]
public class TodoController : Controller
{
    private readonly AppDbContext _db;

    public TodoController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var today = DateTime.Today;
        var y = year ?? today.Year;
        var m = month ?? today.Month;
        if (m < 1) { m = 12; y--; }
        if (m > 12) { m = 1; y++; }

        var userId = GetCurrentUserId();

        var firstOfMonth = new DateOnly(y, m, 1);
        // Lưới lịch bắt đầu từ Thứ 2 của tuần chứa ngày 1 (quy ước tuần VN bắt đầu Thứ 2).
        var isoWeekday = (int)firstOfMonth.DayOfWeek == 0 ? 7 : (int)firstOfMonth.DayOfWeek; // Chủ nhật=7
        var gridStart = firstOfMonth.AddDays(-(isoWeekday - 1));
        var gridEnd = gridStart.AddDays(41); // 6 tuần x 7 ngày - 1

        var todos = await _db.TodoItems
            .Where(t => t.UserId == userId && t.DueDate >= gridStart && t.DueDate <= gridEnd)
            .OrderBy(t => t.IsCompleted).ThenBy(t => t.Title)
            .ToListAsync();
        var todosByDate = todos.GroupBy(t => t.DueDate).ToDictionary(g => g.Key, g => g.ToList());

        var days = new List<TodoDayCellDto>();
        for (var d = gridStart; d <= gridEnd; d = d.AddDays(1))
        {
            todosByDate.TryGetValue(d, out var dayTodos);
            days.Add(new TodoDayCellDto
            {
                Date = d,
                IsCurrentMonth = d.Month == m,
                IsToday = d == DateOnly.FromDateTime(today),
                Items = (dayTodos ?? new List<TodoItem>())
                    .Select(t => new TodoItemDto { Id = t.Id, Title = t.Title, Note = t.Note, IsCompleted = t.IsCompleted, Priority = t.Priority })
                    .ToList()
            });
        }

        var totalPending = await _db.TodoItems.CountAsync(t => t.UserId == userId && !t.IsCompleted);
        var totalOverdue = await _db.TodoItems.CountAsync(t =>
            t.UserId == userId && !t.IsCompleted && t.DueDate < DateOnly.FromDateTime(today));

        var vi = new CultureInfo("vi-VN");
        var vm = new TodoCalendarViewModel
        {
            Year = y,
            Month = m,
            MonthLabel = firstOfMonth.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy", vi),
            Days = days,
            TotalPending = totalPending,
            TotalOverdue = totalOverdue
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTodoViewModel model, int year, int month)
    {
        if (ModelState.IsValid)
        {
            _db.TodoItems.Add(new TodoItem
            {
                UserId = GetCurrentUserId(),
                Title = InputSanitizer.StripHtml(model.Title) ?? model.Title.Trim(),
                Note = InputSanitizer.StripHtml(model.Note),
                DueDate = model.DueDate,
                Priority = model.Priority,
                EnableReminder = model.EnableReminder,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            TempData["Info"] = "Đã thêm việc cần làm.";
        }
        else
        {
            TempData["Error"] = "Vui lòng nhập đủ tiêu đề và ngày đến hạn.";
        }

        return RedirectToAction(nameof(Index), new { year, month });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        var userId = GetCurrentUserId();
        var todo = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (todo is null) return NotFound();

        todo.IsCompleted = !todo.IsCompleted;
        todo.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isCompleted = todo.IsCompleted });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int year, int month)
    {
        var userId = GetCurrentUserId();
        var todo = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (todo is not null)
        {
            _db.TodoItems.Remove(todo);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index), new { year, month });
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}