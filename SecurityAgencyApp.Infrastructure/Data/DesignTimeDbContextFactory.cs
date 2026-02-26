using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SecurityAgencyApp.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Get the base path - look for appsettings.json in API project or Infrastructure project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SecurityAgencyApp.API");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback: try Infrastructure appsettings.json
            var infraPath = Path.Combine(Directory.GetCurrentDirectory());
            var infraConfig = new ConfigurationBuilder()
                .SetBasePath(infraPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            
            connectionString = infraConfig.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
        }

        // Build DbContext options (same provider as runtime: Database:Provider in appsettings)
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseConfiguredProvider(configuration);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
