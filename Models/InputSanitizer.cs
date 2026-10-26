using System.Text.RegularExpressions;

namespace LearnJP.Models;

/// <summary>Loại bỏ thẻ HTML/script khỏi input tự do để chống XSS (AC: Cập nhật thông tin tài khoản).</summary>
public static class InputSanitizer
{
    private static readonly Regex HtmlTagRegex = new(@"<.*?>", RegexOptions.Compiled);

    public static string? StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var noTags = HtmlTagRegex.Replace(input, string.Empty);
        return noTags.Replace("<", "").Replace(">", "").Trim();
    }
}
