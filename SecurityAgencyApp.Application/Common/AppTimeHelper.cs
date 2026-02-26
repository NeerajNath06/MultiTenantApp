using System.Runtime.CompilerServices;

namespace SecurityAgencyApp.Application.Common;

/// <summary>
/// Application time in a configurable timezone (default: India). Used for attendance check-in/check-out,
/// shift windows, and notifications. Default is set from App:TimeZone at startup; mobile/web can override
/// per request via X-Timezone header (e.g. "Asia/Kolkata" or "India Standard Time").
/// </summary>
public static class AppTimeHelper
{
    private static readonly AsyncLocal<string?> RequestTimeZoneOverride = new();
    private static volatile string _defaultTimeZoneId = "India Standard Time";

    /// <summary>Set default timezone for the app (e.g. from appsettings "App:TimeZone"). Call at startup. Default is India.</summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void SetDefaultTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)) return;
        _defaultTimeZoneId = timeZoneId.Trim();
    }

    /// <summary>Override timezone for the current request (e.g. from X-Timezone header). Set by middleware; clear when request ends.</summary>
    public static void SetRequestTimeZone(string? timeZoneId)
    {
        RequestTimeZoneOverride.Value = string.IsNullOrWhiteSpace(timeZoneId) ? null : timeZoneId.Trim();
    }

    /// <summary>Get the effective timezone id for current context (request override or default).</summary>
    public static string GetEffectiveTimeZoneId()
    {
        return RequestTimeZoneOverride.Value ?? _defaultTimeZoneId;
    }

    private static TimeZoneInfo GetTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            if (string.Equals(timeZoneId, "India Standard Time", StringComparison.OrdinalIgnoreCase))
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            if (string.Equals(timeZoneId, "Asia/Kolkata", StringComparison.OrdinalIgnoreCase))
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            throw;
        }
    }

    /// <summary>Current date and time in app timezone (Kind = Unspecified). Use for CheckInTime, CheckOutTime, and "today" for attendance.</summary>
    public static DateTime NowInAppTimeZone()
    {
        var tz = GetTimeZone(GetEffectiveTimeZoneId());
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
    }

    /// <summary>Today's date in app timezone (start of day). Use for AttendanceDate.</summary>
    public static DateTime TodayInAppTimeZone()
    {
        return NowInAppTimeZone().Date;
    }

    /// <summary>Convert UTC to app timezone (for when client sends UTC).</summary>
    public static DateTime UtcToAppTimeZone(DateTime utc)
    {
        if (utc.Kind != DateTimeKind.Utc)
            utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var tz = GetTimeZone(GetEffectiveTimeZoneId());
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }
}
