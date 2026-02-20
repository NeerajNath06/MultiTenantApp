using System.Collections.Concurrent;

namespace SecurityAgencyApp.API.Middleware;

/// <summary>Enterprise: simple per-tenant rate limit (e.g. 600 requests per minute per tenant).</summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _store = new();
    private static readonly ConcurrentDictionary<string, object> _keyLocks = new();
    private const int MaxRequestsPerMinute = 600;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public RateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = context.Request.Headers["X-Tenant-Id"].FirstOrDefault()
            ?? context.User?.FindFirst("TenantId")?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "global";
        var now = DateTime.UtcNow;
        var lockObj = _keyLocks.GetOrAdd(key, _ => new object());
        RateLimitEntry entry;
        lock (lockObj)
        {
            if (!_store.TryGetValue(key, out var e) || now - e.WindowStart > Window)
                entry = new RateLimitEntry(1, now);
            else
                entry = new RateLimitEntry(e.Count + 1, e.WindowStart);
            _store[key] = entry;
        }
        if (entry.Count > MaxRequestsPerMinute)
        {
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Too many requests. Please try again later.",
                data = (object?)null,
                errors = new[] { "Rate limit exceeded." },
                timestamp = DateTime.UtcNow
            }));
            return;
        }
        await _next(context);
    }

    private class RateLimitEntry
    {
        public int Count;
        public readonly DateTime WindowStart;
        public RateLimitEntry(int count, DateTime windowStart) { Count = count; WindowStart = windowStart; }
    }
}
