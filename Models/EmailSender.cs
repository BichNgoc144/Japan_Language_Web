using System.Net;
using System.Net.Mail;

namespace LearnJP.Models;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "no-reply@learnjp.com";
    public string FromName { get; set; } = "LearnJP";

    /// <summary>true = chỉ log ra console (dev local chưa cấu hình SMTP thật) thay vì gửi email thật.</summary>
    public bool UseConsoleOnly { get; set; } = true;
}

/// <summary>
/// Gửi email bằng System.Net.Mail.SmtpClient — có sẵn trong .NET base class library,
/// không cần cài thêm package ngoài (MailKit...), phù hợp môi trường không truy cập được NuGet.
/// </summary>
public class EmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(SmtpSettings settings, ILogger<EmailSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public Task SendOtpEmailAsync(string toEmail, string otpCode)
    {
        var subject = "Mã xác thực OTP của bạn";
        var body = $"<p>Mã OTP của bạn là: <b>{otpCode}</b></p><p>Mã có hiệu lực trong 5 phút. Không chia sẻ mã này cho bất kỳ ai.</p>";
        return SendAsync(toEmail, subject, body);
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "Yêu cầu đặt lại mật khẩu";
        var body = $"<p>Nhấn vào liên kết sau để đặt lại mật khẩu (hiệu lực 15 phút):</p><p><a href=\"{resetLink}\">{resetLink}</a></p><p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>";
        return SendAsync(toEmail, subject, body);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (_settings.UseConsoleOnly)
        {
            _logger.LogInformation("[DEV-EMAIL] To: {To} | Subject: {Subject} | Body: {Body}", toEmail, subject, htmlBody);
            return;
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = true
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}
