using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Infrastructure.Data;
using SecurityAgencyApp.Infrastructure.Repositories;
using SecurityAgencyApp.Infrastructure.Services;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using QuestPDF.Infrastructure;
QuestPDF.Settings.License = LicenseType.Community;

// PostgreSQL: allow DateTime with Kind=Unspecified to be written as UTC (timestamp with time zone)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Security Agency API",
        Version = "v1",
        Description = "API for Security Agency Management System"
    });
    
    // Resolve conflicts with multiple schemas with the same name
    c.CustomSchemaIds(type => type.FullName);
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database: provider from config (SqlServer | PostgreSQL | MySql), connection from config or env
builder.Services.AddApplicationDbContextFromConfig(builder.Configuration);

// Enterprise: in-memory cache for menus/sites (tenant-scoped keys, short TTL)
builder.Services.AddMemoryCache();

// Enterprise: background job – auto-complete assignments/attendance when end date or shift end passes; next-shift notifications
builder.Services.AddHostedService<SecurityAgencyApp.API.HostedServices.AssignmentAndAttendanceBackgroundService>();

// Application Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IJwtService, SecurityAgencyApp.Infrastructure.Identity.JwtService>();
builder.Services.AddScoped<IPasswordHasher, SecurityAgencyApp.Infrastructure.Identity.PasswordHasher>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(SecurityAgencyApp.Application.Common.Models.ApiResponse<>).Assembly);
    cfg.AddOpenBehavior(typeof(SecurityAgencyApp.Application.Common.Behaviors.LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(SecurityAgencyApp.Application.Common.Behaviors.ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(SecurityAgencyApp.Application.Common.Models.ApiResponse<>).Assembly);

// JWT Authentication (enterprise: production – set env vars; no secrets in appsettings)
var jwtSecret = Environment.GetEnvironmentVariable("JwtSettings__SecretKey")
    ?? builder.Configuration["JwtSettings:SecretKey"] ?? "";
var jwtIssuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer")
    ?? builder.Configuration["JwtSettings:Issuer"] ?? "SecurityAgencyApp";
var jwtAudience = Environment.GetEnvironmentVariable("JwtSettings__Audience")
    ?? builder.Configuration["JwtSettings:Audience"] ?? "SecurityAgencyApp";
if (!string.IsNullOrEmpty(jwtSecret))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
}

// CORS (enterprise: restrict origins in production via CORS:AllowedOrigins; empty = AllowAll for dev)
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string>()?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiCors", policy =>
    {
        policy.AllowAnyMethod()
              .AllowAnyHeader();
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
        else
            policy.AllowAnyOrigin();
    });
});

var app = builder.Build();

// Application timezone: default India; mobile/web can send X-Timezone header to override per request
SecurityAgencyApp.Application.Common.AppTimeHelper.SetDefaultTimeZone(
    app.Configuration["App:TimeZone"] ?? "India Standard Time");

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var dbConnectionString = context.Database.GetConnectionString();
        var dbName = GetDatabaseNameFromConnectionString(dbConnectionString);
        logger.LogInformation("Database: {Database}. Applying migrations...", dbName ?? "(unknown)");

        await context.Database.MigrateAsync();
        logger.LogInformation("Migrations applied. Running seed...");

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var seeded = await SecurityAgencyApp.Infrastructure.Data.DbInitializer.SeedAsync(context, passwordHasher);

        if (seeded)
            logger.LogInformation("Database seed completed. Data written to: {Database}.", dbName ?? "(unknown)");
        else
            logger.LogInformation("Seed skipped – database already has data (Tenants table not empty). Using: {Database}.", dbName ?? "(unknown)");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration or seed failed: {Message}", ex.Message);
    }
}

static string? GetDatabaseNameFromConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString)) return null;
    // PostgreSQL / SQL Server: Database=name;
    var key = "Database=";
    var idx = connectionString.IndexOf(key, StringComparison.OrdinalIgnoreCase);
    if (idx >= 0)
    {
        idx += key.Length;
        var end = connectionString.IndexOf(';', idx);
        return end < 0 ? connectionString[idx..].Trim() : connectionString[idx..end].Trim();
    }
    // PostgreSQL URL: postgresql://user:pass@host:port/dbname
    if (connectionString.TrimStart().StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var uri = new Uri(connectionString);
            return uri.AbsolutePath.TrimStart('/').Split('/')[0];
        }
        catch { /* ignore */ }
    }
    return null;
}

// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Security Agency API v1");
    });
//}

app.UseHttpsRedirection();
app.UseCors("ApiCors");

// Enterprise: correlation ID for request tracing (early in pipeline)
app.UseMiddleware<SecurityAgencyApp.API.Middleware.CorrelationIdMiddleware>();
// App timezone: use X-Timezone header from mobile/web if sent; else use App:TimeZone (default India)
app.UseMiddleware<SecurityAgencyApp.API.Middleware.AppTimeZoneMiddleware>();

// Global exception handler: consistent ApiResponse shape for 500 (enterprise)
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var message = ex?.Message ?? "An error occurred.";
        if (app.Environment.IsDevelopment() && ex != null)
            message = ex.ToString();
        var body = System.Text.Json.JsonSerializer.Serialize(new { success = false, message, data = (object?)null, errors = (string[]?)null, timestamp = DateTime.UtcNow });
        await context.Response.WriteAsync(body);
    });
});

app.UseAuthentication();
app.UseMiddleware<SecurityAgencyApp.API.Middleware.TenantContextMiddleware>();
// Enterprise: rate limit per tenant (600 req/min)
app.UseMiddleware<SecurityAgencyApp.API.Middleware.RateLimitMiddleware>();
app.UseAuthorization();

//app.MapHealthChecks("/health");
app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");
