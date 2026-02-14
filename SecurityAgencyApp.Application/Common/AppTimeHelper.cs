namespace SecurityAgencyApp.Application.Common;

/// <summary>
/// Application time in India Standard Time (IST). Used for attendance check-in/check-out
/// so the database stores and displays local time (e.g. 1:55 PM) instead of UTC.
/// </summary>
public static class AppTimeHelper
{
    private static readonly TimeZoneInfo IndiaZone = GetIndiaTimeZone();

    private static TimeZoneInfo GetIndiaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); // Windows
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"); // Linux / macOS
        }
    }

    /// <summary>Current date and time in IST (Kind = Unspecified). Use for CheckInTime, CheckOutTime, and "today" for attendance.</summary>
    public static DateTime NowInAppTimeZone()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndiaZone);
    }

    /// <summary>Today's date in IST (start of day in India). Use for AttendanceDate.</summary>
    public static DateTime TodayInAppTimeZone()
    {
        return NowInAppTimeZone().Date;
    }

    /// <summary>Convert UTC to IST (for when client sends UTC). Exposed for use in command handlers.</summary>
    public static DateTime UtcToAppTimeZone(DateTime utc)
    {
        if (utc.Kind != DateTimeKind.Utc)
            utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, IndiaZone);
    }
}
