namespace ZeeAcom.Web.Helpers;

public static class TimeAgo
{
    /// <summary>"just now", "5 min ago", "3 hours ago", "yesterday", "4 days ago", or a date. Expects a UTC time.</summary>
    public static string From(DateTime utc)
    {
        var when = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var span = DateTime.UtcNow - when;
        return span.TotalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{(int)span.TotalMinutes} min ago",
            < 120 => "an hour ago",
            < 60 * 24 => $"{(int)span.TotalHours} hours ago",
            < 60 * 48 => "yesterday",
            < 60 * 24 * 7 => $"{(int)span.TotalDays} days ago",
            _ => when.ToLocalTime().ToString("d MMM yyyy"),
        };
    }
}
