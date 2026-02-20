namespace SecurityAgencyApp.API.Middleware;

/// <summary>
/// Enterprise: ensures every request has a correlation ID for tracing.
/// Reads X-Correlation-Id from request or generates new; sets response header and HttpContext.Items.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    public const string CorrelationIdHeader = "X-Correlation-Id";
    public const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(correlationId))
            correlationId = Guid.NewGuid().ToString("N");

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                context.Response.Headers.Append(CorrelationIdHeader, correlationId);
            return Task.CompletedTask;
        });
        context.Items[CorrelationIdItemKey] = correlationId;

        using (_logger.BeginScope("{CorrelationId}", correlationId))
        {
            await _next(context);
        }
    }
}
