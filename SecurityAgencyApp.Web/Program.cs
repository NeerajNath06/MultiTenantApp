using Microsoft.EntityFrameworkCore;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Infrastructure.Data;
using SecurityAgencyApp.Infrastructure.Repositories;
using SecurityAgencyApp.Infrastructure.Services;
using FluentValidation;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// API client – Web calls API instead of MediatR/DbContext (lightweight)
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5014";
builder.Services.AddHttpClient<SecurityAgencyApp.Web.Services.IApiClient, SecurityAgencyApp.Web.Services.ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/'));
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Database: provider from config (SqlServer | PostgreSQL), same as API
builder.Services.AddApplicationDbContextFromConfig(builder.Configuration);

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

var app = builder.Build();

// Seed database and ensure new menus exist for existing tenants
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        await SecurityAgencyApp.Infrastructure.Data.DbInitializer.SeedAsync(context, passwordHasher);
        await SecurityAgencyApp.Infrastructure.Data.DbInitializer.EnsureNewMenusAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Set tenant context from session
app.UseMiddleware<SecurityAgencyApp.Web.Middleware.TenantContextMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
