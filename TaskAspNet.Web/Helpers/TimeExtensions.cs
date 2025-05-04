
namespace TaskAspNet.Web.Helpers;

// Created with the help of ChatGPT4.5
// Provides a human-friendly relative time string for a DateTime
// Calculates the timespan between now and the given date (in UTC)
// If under 60 seconds, returns just now
// If under 60 minutes, returns minute
// If under 24 hours, returns hour
// If under 7 days, returns day
// If under 30 days, returns week
// If under 365 days, returns month
// Otherwise returns year
// Adds an s to the end of the string if the time is plural

public static class TimeExtensions
{
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var ts = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (ts.TotalSeconds < 60)
            return "just now";

        if (ts.TotalMinutes < 60)
            return $"{(int)ts.TotalMinutes} minute{(ts.TotalMinutes >= 2 ? "s" : "")} ago";

        if (ts.TotalHours < 24)
            return $"{(int)ts.TotalHours} hour{(ts.TotalHours >= 2 ? "s" : "")} ago";

        if (ts.TotalDays < 7)
            return $"{(int)ts.TotalDays} day{(ts.TotalDays >= 2 ? "s" : "")} ago";

        if (ts.TotalDays < 30)
            return $"{(int)(ts.TotalDays / 7)} week{(ts.TotalDays >= 14 ? "s" : "")} ago";

        if (ts.TotalDays < 365)
            return $"{(int)(ts.TotalDays / 30)} month{(ts.TotalDays >= 60 ? "s" : "")} ago";

        return $"{(int)(ts.TotalDays / 365)} year{(ts.TotalDays >= 730 ? "s" : "")} ago";
    }
}
