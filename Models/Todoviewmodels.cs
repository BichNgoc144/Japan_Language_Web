using System.ComponentModel.DataAnnotations;

namespace LearnJP.Models;

public class TodoItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsCompleted { get; set; }
    public TodoPriority Priority { get; set; }
}

/// <summary>1 ô ngày trong bảng lịch — có thể thuộc tháng trước/sau (để lấp đầy lưới 6 hàng x 7 cột).</summary>
public class TodoDayCellDto
{
    public DateOnly Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public List<TodoItemDto> Items { get; set; } = new();
}

public class TodoCalendarViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty; // "Tháng 7, 2026"
    public List<TodoDayCellDto> Days { get; set; } = new(); // đúng 42 ô (6 tuần)
    public int TotalPending { get; set; }
    public int TotalOverdue { get; set; }
}

public class CreateTodoViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [MaxLength(200)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày")]
    [Display(Name = "Ngày đến hạn")]
    [DataType(DataType.Date)]
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Độ ưu tiên")]
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    /// <summary>Chỉ khi tick mới hiện ở banner thông báo trang chủ.</summary>
    [Display(Name = "Nhắc nhở tôi ở trang chủ")]
    public bool EnableReminder { get; set; } = false;
}

/// <summary>1 việc sắp/đã đến hạn có tick "Nhắc nhở" — hiển thị ở banner thông báo trang chủ.</summary>
public class TodoReminderDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsToday { get; set; }
}