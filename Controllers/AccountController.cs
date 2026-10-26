using System.Security.Claims;
using LearnJP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnJP.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly EmailSender _emailSender;

    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);

    // Khoá tài khoản sau nhiều lần đăng nhập sai (AC đã chốt ở epic này).
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public AccountController(AppDbContext db, EmailSender emailSender)
    {
        _db = db;
        _emailSender = emailSender;
    }

    // ================= ĐĂNG KÝ — Bước 1: nhập email, gửi OTP =================

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterEmailViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterEmailViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();

        // AC: Email không được trùng lặp trong hệ thống.
        var emailExists = await _db.Users.AnyAsync(u => u.Email == email);
        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký. Vui lòng đăng nhập hoặc dùng email khác.");
            return View(model);
        }

        // Vô hiệu hoá mọi OTP Register cũ chưa dùng của cùng email, tránh tồn tại nhiều mã hợp lệ song song.
        var oldOtps = await _db.OtpVerifications
            .Where(o => o.Email == email && o.Purpose == OtpPurpose.Register && !o.IsUsed)
            .ToListAsync();
        foreach (var old in oldOtps) old.IsUsed = true;

        var otpCode = PasswordHasher.GenerateOtpCode();
        _db.OtpVerifications.Add(new OtpVerification
        {
            Email = email,
            Purpose = OtpPurpose.Register,
            OtpCodeHash = PasswordHasher.Hash(otpCode),
            ExpiresAt = DateTime.UtcNow.Add(OtpLifetime)
        });
        await _db.SaveChangesAsync();

        await _emailSender.SendOtpEmailAsync(email, otpCode);

        TempData["Info"] = "Đã gửi mã OTP tới email của bạn.";
        return RedirectToAction(nameof(VerifyOtp), new { email });
    }

    // ================= ĐĂNG KÝ — Bước 2: xác thực OTP =================

    [HttpGet]
    public IActionResult VerifyOtp(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Register));
        return View(new VerifyOtpViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();

        var otp = await _db.OtpVerifications
            .Where(o => o.Email == email && o.Purpose == OtpPurpose.Register && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp is null)
        {
            ModelState.AddModelError(string.Empty, "Không tìm thấy yêu cầu OTP hợp lệ. Vui lòng yêu cầu gửi lại mã mới.");
            return View(model);
        }
        if (otp.ExpiresAt < DateTime.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Mã OTP đã hết hạn. Vui lòng yêu cầu gửi lại mã mới.");
            return View(model);
        }
        if (otp.AttemptCount >= OtpVerification.MaxAttempts)
        {
            otp.IsUsed = true;
            await _db.SaveChangesAsync();
            ModelState.AddModelError(string.Empty, "Bạn đã nhập sai OTP quá số lần cho phép. Vui lòng yêu cầu gửi lại mã mới.");
            return View(model);
        }

        var isMatch = PasswordHasher.Verify(model.OtpCode, otp.OtpCodeHash);
        if (!isMatch)
        {
            otp.AttemptCount++;
            if (otp.AttemptCount >= OtpVerification.MaxAttempts) otp.IsUsed = true;
            await _db.SaveChangesAsync();

            var remaining = Math.Max(0, OtpVerification.MaxAttempts - otp.AttemptCount);
            ModelState.AddModelError(string.Empty, $"Mã OTP không đúng. Bạn còn {remaining} lần thử.");
            return View(model);
        }

        otp.IsVerified = true;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(CreatePassword), new { email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Register));
        // Dùng lại đúng luồng gửi OTP của bước 1.
        return await Register(new RegisterEmailViewModel { Email = email });
    }

    // ================= ĐĂNG KÝ — Bước 3: tạo username + mật khẩu =================

    [HttpGet]
    public async Task<IActionResult> CreatePassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return RedirectToAction(nameof(Register));

        var verified = await IsEmailOtpVerifiedAsync(email);
        if (!verified)
        {
            TempData["Error"] = "Email chưa được xác thực OTP. Vui lòng thực hiện lại từ đầu.";
            return RedirectToAction(nameof(Register));
        }

        return View(new CreatePasswordViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePassword(CreatePasswordViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();

        var verified = await IsEmailOtpVerifiedAsync(email);
        if (!verified)
        {
            TempData["Error"] = "Phiên xác thực đã hết hạn. Vui lòng thực hiện lại từ đầu.";
            return RedirectToAction(nameof(Register));
        }

        if (!ModelState.IsValid) return View(model);

        var usernameTaken = await _db.Users.AnyAsync(u => u.Username == model.Username);
        if (usernameTaken)
        {
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập này đã được sử dụng.");
            return View(model);
        }
        var emailTaken = await _db.Users.AnyAsync(u => u.Email == email);
        if (emailTaken)
        {
            ModelState.AddModelError(string.Empty, "Email này đã được đăng ký.");
            return View(model);
        }

        var user = new User
        {
            Username = model.Username.Trim(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(model.Password),
            Role = UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(); // cần UserId trước khi tạo 2 bảng con

        _db.UserDetails.Add(new UserDetail
        {
            UserId = user.UserId,
            FullName = InputSanitizer.StripHtml(model.FullName)
        });
        _db.UserLearns.Add(new UserLearn
        {
            UserId = user.UserId,
            Level = 0,
            Point = 0,
            Time = DateTime.UtcNow
        });

        // Đánh dấu đã dùng OTP để không thể tái sử dụng cho lần đăng ký khác.
        var otp = await _db.OtpVerifications
            .Where(o => o.Email == email && o.Purpose == OtpPurpose.Register && o.IsVerified && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
        if (otp is not null) otp.IsUsed = true;

        await _db.SaveChangesAsync();

        TempData["Info"] = "Đăng ký thành công! Hãy đăng nhập.";
        return RedirectToAction("Login");
    }

    /// <summary>Sinh gợi ý mật khẩu mạnh cho form tạo mật khẩu (gọi qua fetch từ View, trả JSON).</summary>
    [HttpGet]
    public IActionResult GeneratePassword()
    {
        return Json(new { password = PasswordHasher.GenerateStrongPassword() });
    }

    // ================= ĐĂNG NHẬP / ĐĂNG XUẤT =================

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Profile));
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // AC: sai mật khẩu quá 5 lần -> tạm khoá tài khoản 15 phút.
        if (user.LockoutEndAt.HasValue && user.LockoutEndAt > DateTime.UtcNow)
        {
            var minutesLeft = Math.Ceiling((user.LockoutEndAt.Value - DateTime.UtcNow).TotalMinutes);
            ModelState.AddModelError(string.Empty, $"Tài khoản tạm khoá do đăng nhập sai quá nhiều lần. Thử lại sau {minutesLeft} phút.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị khoá bởi quản trị viên.");
            return View(model);
        }

        var passwordOk = PasswordHasher.Verify(model.Password, user.PasswordHash);
        if (!passwordOk)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
            {
                user.LockoutEndAt = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
            }
            await _db.SaveChangesAsync();
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // Đăng nhập thành công: reset bộ đếm.
        user.FailedLoginAttempts = 0;
        user.LockoutEndAt = null;
        await _db.SaveChangesAsync();

        await SignInUserAsync(user, model.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // AC: hủy phiên đăng nhập khi đăng xuất (xoá cookie xác thực).
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Info"] = "Đã đăng xuất.";
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Tạo cookie xác thực. Claim "stamp" mang theo User.SecurityStamp tại thời điểm đăng nhập —
    /// khi đổi/reset mật khẩu, stamp trong DB sẽ đổi, khiến cookie cũ (mang stamp cũ) bị coi là
    /// không hợp lệ ở lần request tiếp theo (kiểm tra trong sự kiện OnValidatePrincipal, cấu hình ở Program.cs).
    /// </summary>
    private async Task SignInUserAsync(User user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("stamp", user.SecurityStamp)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    // ================= QUÊN MẬT KHẨU =================

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Không tiết lộ email có tồn tại hay không — luôn hiện thông báo thành công.
        if (user is not null)
        {
            var rawToken = PasswordHasher.GenerateRawToken();
            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.UserId,
                TokenHash = PasswordHasher.HashToken(rawToken),
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });
            await _db.SaveChangesAsync();

            var resetLink = Url.Action(nameof(ResetPassword), "Account", new { token = rawToken }, Request.Scheme)!;
            await _emailSender.SendPasswordResetEmailAsync(email, resetLink);
        }

        TempData["Info"] = "Nếu email tồn tại trong hệ thống, liên kết đặt lại mật khẩu đã được gửi. Hãy kiểm tra hộp thư.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    // ================= ĐẶT LẠI MẬT KHẨU (link từ email) =================

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Liên kết không hợp lệ.";
            return RedirectToAction(nameof(ForgotPassword));
        }
        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var tokenHash = PasswordHasher.HashToken(model.Token);
        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (resetToken is null || resetToken.User is null)
        {
            ModelState.AddModelError(string.Empty, "Liên kết đặt lại mật khẩu không hợp lệ.");
            return View(model);
        }
        // AC: link chỉ dùng được 1 lần + có thời hạn 15 phút.
        if (resetToken.IsUsed)
        {
            ModelState.AddModelError(string.Empty, "Liên kết này đã được sử dụng.");
            return View(model);
        }
        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Liên kết đặt lại mật khẩu đã hết hạn. Vui lòng yêu cầu gửi lại.");
            return View(model);
        }

        resetToken.User.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        resetToken.User.UpdatedAt = DateTime.UtcNow;
        // Đổi SecurityStamp -> mọi cookie đăng nhập cũ trên các thiết bị khác lập tức bị vô hiệu hoá.
        resetToken.User.SecurityStamp = Guid.NewGuid().ToString("N");
        resetToken.IsUsed = true;

        await _db.SaveChangesAsync();

        TempData["Info"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.";
        return RedirectToAction(nameof(Login));
    }

    // ================= ĐỔI MẬT KHẨU (đã đăng nhập) =================

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetCurrentUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null) return RedirectToAction(nameof(Login));

        // AC: bắt buộc nhập đúng mật khẩu cũ.
        if (!PasswordHasher.Verify(model.OldPassword, user.PasswordHash))
        {
            ModelState.AddModelError(nameof(model.OldPassword), "Mật khẩu hiện tại không đúng.");
            return View(model);
        }

        user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N"); // vô hiệu hoá mọi cookie cũ, kể cả thiết bị khác
        await _db.SaveChangesAsync();

        // Cookie hiện tại cũng mang stamp cũ -> ký lại ngay để không tự đá chính mình ra ngoài.
        await SignInUserAsync(user, rememberMe: false);

        TempData["Info"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }

    // ================= CẬP NHẬT THÔNG TIN TÀI KHOẢN =================

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        var user = await _db.Users
            .Include(u => u.UserDetail)
            .Include(u => u.UserLearn)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null) return RedirectToAction(nameof(Login));

        var detail = user.UserDetail;
        ViewData["Username"] = user.Username;
        ViewData["Email"] = user.Email;
        ViewData["Level"] = user.UserLearn?.Level ?? 0;
        ViewData["Point"] = user.UserLearn?.Point ?? 0;

        return View(new EditProfileViewModel
        {
            FullName = detail?.FullName,
            Gender = detail?.Gender,
            DateOfBirth = detail?.DateOfBirth,
            Phone = detail?.Phone,
            Address = detail?.Address,
            University = detail?.University,
            Major = detail?.Major,
            Bio = detail?.Bio
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Profile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = GetCurrentUserId();
        var detail = await _db.UserDetails.FirstOrDefaultAsync(d => d.UserId == userId);
        if (detail is null)
        {
            // Phòng hờ dữ liệu cũ chưa có dòng userdetails tương ứng.
            detail = new UserDetail { UserId = userId };
            _db.UserDetails.Add(detail);
        }

        // AC: validate + chống XSS cho các trường text tự do.
        detail.FullName = InputSanitizer.StripHtml(model.FullName);
        detail.Gender = model.Gender;
        detail.DateOfBirth = model.DateOfBirth;
        detail.Phone = model.Phone?.Trim();
        detail.Address = InputSanitizer.StripHtml(model.Address);
        detail.University = InputSanitizer.StripHtml(model.University);
        detail.Major = InputSanitizer.StripHtml(model.Major);
        detail.Bio = InputSanitizer.StripHtml(model.Bio);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is not null) user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Info"] = "Cập nhật thông tin thành công.";
        return RedirectToAction(nameof(Profile));
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> IsEmailOtpVerifiedAsync(string email)
    {
        email = email.Trim().ToLowerInvariant();
        var otp = await _db.OtpVerifications
            .Where(o => o.Email == email && o.Purpose == OtpPurpose.Register && o.IsVerified && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
        return otp is not null && otp.ExpiresAt >= DateTime.UtcNow;
    }
}
