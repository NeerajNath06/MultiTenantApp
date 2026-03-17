using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SecurityAgencyApp.Web.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddConfiguredJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")
            ?? configuration["JwtSettings:SecretKey"]
            ?? string.Empty;
        var jwtIssuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer")
            ?? configuration["JwtSettings:Issuer"]
            ?? "SecurityAgencyApp";
        var jwtAudience = Environment.GetEnvironmentVariable("JwtSettings__Audience")
            ?? configuration["JwtSettings:Audience"]
            ?? "SecurityAgencyApp";

        if (string.IsNullOrWhiteSpace(jwtSecret))
            return services;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
