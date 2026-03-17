using SecurityAgencyApp.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityAgencyWebServices(builder.Configuration);
builder.Services.AddConfiguredApiVersioning();
builder.Services.AddConfiguredSwagger();
builder.Services.AddConfiguredJwtAuthentication(builder.Configuration);
builder.Services.AddConfiguredCorsPolicy(builder.Configuration);

var app = builder.Build();

app.ConfigureRuntimeDefaults();
await app.InitializeDatabaseAsync();
app.UseApiExceptionHandling();
app.UseSecurityAgencyPipeline();
app.MapSecurityAgencyEndpoints();

app.Run();
