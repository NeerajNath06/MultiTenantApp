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

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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

// JWT Authentication (so tenant can be read from token when X-Tenant-Id missing)
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"] ?? "";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "SecurityAgencyApp";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "SecurityAgencyApp";
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        await SecurityAgencyApp.Infrastructure.Data.DbInitializer.SeedAsync(context, passwordHasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Security Agency API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseMiddleware<SecurityAgencyApp.API.Middleware.TenantContextMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
