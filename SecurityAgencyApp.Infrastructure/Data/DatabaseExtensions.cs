using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SecurityAgencyApp.Infrastructure.Data;

/// <summary>
/// Database provider types. Use appsettings "Database:Provider" (e.g. "PostgreSQL", "SqlServer", "MySql").
/// </summary>
public static class DatabaseProvider
{
    public const string SqlServer = "SqlServer";
    public const string PostgreSQL = "PostgreSQL";
    public const string MySql = "MySql";
}

public static class DatabaseExtensions
{
    /// <summary>
    /// Adds ApplicationDbContext configured for the provider specified in configuration.
    /// Config keys: "Database:Provider" (SqlServer | PostgreSQL | MySql), "ConnectionStrings:DefaultConnection".
    /// </summary>
    public static IServiceCollection AddApplicationDbContextFromConfig(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var provider = configuration["Database:Provider"]
            ?? Environment.GetEnvironmentVariable("Database__Provider")
            ?? DatabaseProvider.SqlServer;

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set ConnectionStrings:DefaultConnection or env ConnectionStrings__DefaultConnection.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (provider.Trim())
            {
                case DatabaseProvider.PostgreSQL:
                    options.UseNpgsql(connectionString);
                    options.ReplaceService<IMigrationsSqlGenerator, NpgsqlCompatibleMigrationsSqlGenerator>();
                    break;
                case DatabaseProvider.SqlServer:
                default:
                    options.UseSqlServer(connectionString);
                    break;
                case DatabaseProvider.MySql:
                    throw new NotSupportedException("MySql: add Pomelo.EntityFrameworkCore.MySql package and configure UseMySql here.");
            }
        });

        return services;
    }

    /// <summary>
    /// Configures DbContextOptionsBuilder with the correct provider (for design-time / migrations).
    /// </summary>
    public static DbContextOptionsBuilder<ApplicationDbContext> UseConfiguredProvider(
        this DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var provider = configuration["Database:Provider"]?.Trim() ?? DatabaseProvider.SqlServer;

        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                optionsBuilder.UseNpgsql(connectionString);
                optionsBuilder.ReplaceService<IMigrationsSqlGenerator, NpgsqlCompatibleMigrationsSqlGenerator>();
                break;
            case DatabaseProvider.SqlServer:
            default:
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case DatabaseProvider.MySql:
                throw new NotSupportedException("MySql: add Pomelo.EntityFrameworkCore.MySql and configure here.");
        }
        return optionsBuilder;
    }
}
