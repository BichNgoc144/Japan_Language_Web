using System.ComponentModel.DataAnnotations;

namespace LearnJP.Models;

// ---------- Quên mật khẩu ----------
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

// ---------- Đặt lại mật khẩu (từ link email) ----------
public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [Display(Name = "Nhập lại mật khẩu mới")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// ---------- Đổi mật khẩu (đã đăng nhập) ----------
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [Display(Name = "Nhập lại mật khẩu mới")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// ---------- Cập nhật thông tin tài khoản ----------
public class EditProfileViewModel
{
    [StringLength(100)]
    [Display(Name = "Họ và tên")]
    public string? FullName { get; set; }

    [Display(Name = "Giới tính")]
    public Gender? Gender { get; set; }

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(255)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [StringLength(100)]
    [Display(Name = "Trường học")]
    public string? University { get; set; }

    [StringLength(100)]
    [Display(Name = "Chuyên ngành")]
    public string? Major { get; set; }

    [Display(Name = "Giới thiệu bản thân")]
    public string? Bio { get; set; }
}

// ---------- Đăng nhập ----------
public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; } = false;
}

// ---------- Đăng ký — bước 1: nhập email ----------
public class RegisterEmailViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

// ---------- Đăng ký — bước 2: xác thực OTP ----------
public class VerifyOtpViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP phải gồm đúng 6 chữ số")]
    [Display(Name = "Mã OTP")]
    public string OtpCode { get; set; } = string.Empty;
}

// ---------- Đăng ký — bước 3: tạo username + mật khẩu ----------
public class CreatePasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username từ 3-50 ký tự")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ gồm chữ, số và dấu gạch dưới")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    [Display(Name = "Nhập lại mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Họ và tên (tuỳ chọn)")]
    [StringLength(100)]
    public string? FullName { get; set; }
}
