namespace SecurityAgencyApp.Web.Extensions;

public static class CorsExtensions
{
    public const string ApiCorsPolicyName = "ApiCors";

    public static IServiceCollection AddConfiguredCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string>()?
            .Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(ApiCorsPolicyName, policy =>
            {
                policy.AllowAnyMethod()
                    .AllowAnyHeader();

                if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins);
                else
                    policy.AllowAnyOrigin();
            });
        });

        return services;
    }
}
