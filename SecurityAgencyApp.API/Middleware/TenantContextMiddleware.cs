using System.Security.Claims;
using System.Text.Json;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Infrastructure.Services;

namespace SecurityAgencyApp.API.Middleware;

/// <summary>
/// Sets tenant context from X-Tenant-Id header (or from JWT TenantId claim if header missing).
/// Enterprise: for authenticated API requests (except Auth and health), rejects with 403 if TenantId is missing.
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantContext = context.RequestServices.GetService<ITenantContext>();
        if (tenantContext is not TenantContext tc)
        {
            await _next(context);
            return;
        }

        var tenantIdHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tenantIdHeader) && Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            tc.TenantId = tenantId;
        }
        else if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var fromToken))
                tc.TenantId = fromToken;
        }

        // Set current user context from JWT (so handlers like CreateAssignment have UserId for CreatedBy)
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var currentUserService = context.RequestServices.GetService<ICurrentUserService>();
            if (currentUserService is CurrentUserService cus)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    cus.UserId = userId;
                    cus.UserName = context.User.FindFirst(ClaimTypes.Name)?.Value;
                    cus.Email = context.User.FindFirst(ClaimTypes.Email)?.Value;
                }
            }
        }

        // Enterprise: mandatory tenant on protected API routes (authenticated + tenant-scoped)
        var path = context.Request.Path.Value ?? "";
        if (context.User?.Identity?.IsAuthenticated == true &&
            path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith("/api/v1/Auth", StringComparison.OrdinalIgnoreCase) &&
            !path.Equals("/health", StringComparison.OrdinalIgnoreCase) &&
            !tc.TenantId.HasValue)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var body = JsonSerializer.Serialize(new
            {
                success = false,
                message = "Tenant context is required. Provide X-Tenant-Id header or ensure your token includes TenantId.",
                data = (object?)null,
                errors = new[] { "Missing or invalid tenant context." },
                timestamp = DateTime.UtcNow
            });
            await context.Response.WriteAsync(body);
            return;
        }

        await _next(context);
    }
}
