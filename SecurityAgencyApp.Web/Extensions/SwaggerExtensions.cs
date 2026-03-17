using Microsoft.OpenApi.Models;

namespace SecurityAgencyApp.Web.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddConfiguredSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Security Agency API",
                Version = "v1",
                Description = "API for Security Agency Management System"
            });
            options.CustomSchemaIds(type => type.FullName);
        });

        return services;
    }

    public static WebApplication UseConfiguredSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Security Agency API v1");
        });

        return app;
    }
}
