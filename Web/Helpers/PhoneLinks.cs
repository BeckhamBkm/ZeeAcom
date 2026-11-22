namespace ZeeAcom.Web.Helpers;

/// <summary>Call and WhatsApp links for South African cell numbers.</summary>
public static class PhoneLinks
{
    /// <summary>"082 123 4567" / "+27 82 123 4567" -> "27821234567"; null if it doesn't look like an SA number.</summary>
    public static string? InternationalDigits(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 10 && digits.StartsWith('0'))
        {
            return "27" + digits[1..];
        }

        return digits.Length == 11 && digits.StartsWith("27") ? digits : null;
    }

    public static string? Tel(string? phone)
        => InternationalDigits(phone) is { } digits ? $"tel:+{digits}" : null;

    /// <summary>wa.me link that opens a chat with a pre-filled message.</summary>
    public static string? WhatsApp(string? phone, string message)
        => InternationalDigits(phone) is { } digits ? $"https://wa.me/{digits}?text={Uri.EscapeDataString(message)}" : null;
}
