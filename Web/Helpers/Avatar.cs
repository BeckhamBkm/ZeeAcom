namespace ZeeAcom.Web.Helpers;

public static class Avatar
{
    /// <summary>Up to two initials for an avatar circle, e.g. "Lerato M" -> "LM".</summary>
    public static string Initials(string? name)
    {
        var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => char.ToUpperInvariant(parts[0][0]).ToString(),
            _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[^1][0])}",
        };
    }
}
