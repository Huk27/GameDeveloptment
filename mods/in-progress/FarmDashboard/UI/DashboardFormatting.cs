using System;
using System.Globalization;
using FarmDashboard.Data;

namespace FarmDashboard.UI;

internal static class DashboardFormatting
{
    public static string FormatMoney(int value)
    {
        return string.Format(CultureInfo.CurrentCulture, "{0:N0}g", value);
    }

    public static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalMinutes < 1)
            return "0m";

        if (span.TotalHours < 1)
            return string.Format(CultureInfo.CurrentCulture, "{0}m", span.Minutes);

        return string.Format(CultureInfo.CurrentCulture, "{0}h {1}m", (int)span.TotalHours, span.Minutes);
    }

    public static string FormatActivityName(ActivityType activity)
    {
        return activity switch
        {
            ActivityType.Farming => "Farming",
            ActivityType.Mining => "Mining",
            ActivityType.Fishing => "Fishing",
            ActivityType.Combat => "Combat",
            ActivityType.Foraging => "Foraging",
            ActivityType.Social => "Social",
            _ => "Other"
        };
    }

    public static string BuildBar(float ratio)
    {
        const int totalBlocks = 12;
        int filled = (int)Math.Round(Math.Clamp(ratio, 0f, 1f) * totalBlocks);
        if (filled <= 0)
            return string.Empty;
        return new string('█', filled);
    }

    public static string GetSeverityColor(string severity)
    {
        return severity switch
        {
            "warning" => "#FF851B",
            "critical" => "#FF4136",
            "good" => "#2ECC40",
            _ => "#FFFFFF"
        };
    }
}
