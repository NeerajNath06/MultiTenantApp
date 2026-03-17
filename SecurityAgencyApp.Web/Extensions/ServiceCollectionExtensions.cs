using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using FluentValidation;
using SecurityAgencyApp.Application.Common.Behaviors;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Infrastructure.Data;
using SecurityAgencyApp.Infrastructure.Repositories;
using SecurityAgencyApp.Infrastructure.Services;

namespace SecurityAgencyApp.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityAgencyWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        services.AddHttpContextAccessor();
        services.AddApplicationDbContextFromConfig(configuration);
        services.AddMemoryCache();
        services.AddHostedService<SecurityAgencyApp.API.HostedServices.AssignmentAndAttendanceBackgroundService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, SecurityAgencyApp.Infrastructure.Identity.JwtService>();
        services.AddScoped<IPasswordHasher, SecurityAgencyApp.Infrastructure.Identity.PasswordHasher>();
        services.AddScoped<SecurityAgencyApp.API.Services.MonthlyReportService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApiResponse<>).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ApiResponse<>).Assembly);

        services.AddHttpClient<SecurityAgencyApp.Web.Services.IApiClient, SecurityAgencyApp.Web.Services.ApiClient>(client =>
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        return services;
    }
}
