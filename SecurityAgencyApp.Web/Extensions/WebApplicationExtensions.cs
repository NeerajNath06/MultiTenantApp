using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SecurityAgencyApp.Application.Common;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Infrastructure.Data;

namespace SecurityAgencyApp.Web.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureRuntimeDefaults(this WebApplication app)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppTimeHelper.SetDefaultTimeZone(app.Configuration["App:TimeZone"] ?? "India Standard Time");

        return app;
    }

    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var passwordHasher = services.GetRequiredService<IPasswordHasher>();
            await DbInitializer.SeedAsync(context, passwordHasher);
            await DbInitializer.EnsureNewMenusAsync(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration or seed failed: {Message}", ex.Message);
        }

        return app;
    }

    public static WebApplication UseApiExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.Redirect("/Home/Error");
                    return;
                }

                var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var statusCode = StatusCodes.Status500InternalServerError;
                var message = "An error occurred.";
                List<ApiError>? errors = null;

                if (ex is ValidationException validationException)
                {
                    statusCode = StatusCodes.Status400BadRequest;
                    message = "Validation failed";
                    errors = validationException.Errors
                        .Select(error => ApiError.Create(error.ErrorMessage, error.PropertyName))
                        .ToList();
                }
                else if (ex is UnauthorizedAccessException)
                {
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = ex.Message;
                }
                else if (ex != null)
                {
                    message = app.Environment.IsDevelopment() ? ex.Message : "An unexpected error occurred.";
                }

                var body = JsonSerializer.Serialize(ApiResponse<object?>.ErrorResponse(message, errors));

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(body);
            });
        });

        return app;
    }

    public static WebApplication UseSecurityAgencyPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            app.UseHsts();
        else
            app.UseDeveloperExceptionPage();

        app.UseConfiguredSwagger();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors(CorsExtensions.ApiCorsPolicyName);
        app.UseSession();
        app.UseMiddleware<SecurityAgencyApp.API.Middleware.CorrelationIdMiddleware>();
        app.UseMiddleware<SecurityAgencyApp.API.Middleware.AppTimeZoneMiddleware>();
        app.UseAuthentication();
        app.UseMiddleware<SecurityAgencyApp.API.Middleware.TenantContextMiddleware>();
        app.UseMiddleware<SecurityAgencyApp.API.Middleware.RateLimitMiddleware>();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapSecurityAgencyEndpoints(this WebApplication app)
    {
        app.MapGet("/check-db", (IConfiguration config) => config.GetConnectionString("DefaultConnection"));
        app.MapControllers();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Auth}/{action=Login}/{id?}");

        return app;
    }
}
