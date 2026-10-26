using LearnJP.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Cấu hình từ appsettings.json ----------
var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:MySql trong appsettings.json");

var smtpSettings = builder.Configuration.GetSection("Smtp").Get<SmtpSettings>() ?? new SmtpSettings();
builder.Services.AddSingleton(smtpSettings);
builder.Services.AddScoped<EmailSender>();

// ---------- DbContext (MySQL qua Pomelo) ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ---------- MVC ----------
builder.Services.AddControllersWithViews();

// Cho phép validate CSRF token gửi qua header (dùng cho fetch AJAX ở trang Kiểm tra từ vựng).
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

// ---------- Cookie Authentication ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // ghi đè theo AuthenticationProperties lúc SignInAsync (RememberMe)
        options.SlidingExpiration = true;

        // Kiểm tra SecurityStamp mỗi request: nếu user đổi/reset mật khẩu ở nơi khác,
        // stamp trong DB đổi -> cookie hiện tại (mang stamp cũ) sẽ bị coi là hết hạn ngay lập tức.
        options.Events.OnValidatePrincipal = async context =>
        {
            var stampClaim = context.Principal?.FindFirst("stamp")?.Value;
            var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (stampClaim is null || userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            {
                context.RejectPrincipal();
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var currentStamp = await db.Users
                .Where(u => u.UserId == userId)
                .Select(u => new { u.SecurityStamp, u.IsActive })
                .FirstOrDefaultAsync();

            if (currentStamp is null || !currentStamp.IsActive || currentStamp.SecurityStamp != stampClaim)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // dự phòng nếu sau này thêm wwwroot; hiện tại toàn bộ CSS đã inline trong _Layout
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Trang gốc "/" vào HomeController (landing page quảng bá).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();