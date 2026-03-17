using Microsoft.AspNetCore.Mvc;

namespace SecurityAgencyApp.Web.Extensions;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddConfiguredApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        return services;
    }
}
