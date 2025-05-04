

namespace TaskAspNet.Web.Helpers;

public static class DateExtensions
{
    public static string ToRemainingTime(this DateTime? endDate)
    {
        if (endDate is null) return "Ongoing";

        int daysLeft = (endDate.Value.Date - DateTime.UtcNow.Date).Days;

        if (daysLeft > 7)
            return $"{daysLeft / 7} weeks left";
        else if (daysLeft > 0)
            return $"{daysLeft} days left";
        else
            return "Deadline passed";
    }

    public static bool IsUrgent(this DateTime? endDate, int thresholdDays = 7)
    {
        if (endDate is null) return false;

        int daysLeft = (endDate.Value - DateTime.Now).Days;  
        return daysLeft >= 0 && daysLeft <= thresholdDays;
    }

}
