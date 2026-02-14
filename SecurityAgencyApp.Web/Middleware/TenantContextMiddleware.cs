using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Infrastructure.Services;

namespace SecurityAgencyApp.Web.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        // Set tenant context from session
        var tenantId = context.Session.GetString("TenantId");
        if (!string.IsNullOrEmpty(tenantId) && Guid.TryParse(tenantId, out var tenantGuid))
        {
            if (tenantContext is TenantContext tc)
            {
                tc.TenantId = tenantGuid;
                tc.TenantName = context.Session.GetString("TenantName");
            }
        }

        // Set current user context from session
        var userId = context.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            if (currentUserService is CurrentUserService cus)
            {
                cus.UserId = userGuid;
                cus.UserName = context.Session.GetString("UserName");
                cus.Email = context.Session.GetString("UserEmail");
            }
        }

        await _next(context);
    }
}
