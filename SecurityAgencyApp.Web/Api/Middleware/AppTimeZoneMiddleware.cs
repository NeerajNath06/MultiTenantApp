using SecurityAgencyApp.Application.Common;

namespace SecurityAgencyApp.API.Middleware;

/// <summary>
/// Reads X-Timezone header from mobile/web (e.g. "Asia/Kolkata" or "India Standard Time")
/// and sets it for the request so all time operations use that timezone. If not sent, app default (e.g. India) is used.
/// </summary>
public class AppTimeZoneMiddleware
{
    private const string TimeZoneHeaderName = "X-Timezone";
    private readonly RequestDelegate _next;

    public AppTimeZoneMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TimeZoneHeaderName, out var value) && !string.IsNullOrWhiteSpace(value))
            AppTimeHelper.SetRequestTimeZone(value.ToString());
        else
            AppTimeHelper.SetRequestTimeZone(null);

        try
        {
            await _next(context);
        }
        finally
        {
            AppTimeHelper.SetRequestTimeZone(null);
        }
    }
}
