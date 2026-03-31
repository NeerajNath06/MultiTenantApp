using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SecurityAgencyApp.Web.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddConfiguredSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            options.DocInclusionPredicate((documentName, apiDescription) =>
                string.Equals(apiDescription.GroupName, documentName, StringComparison.OrdinalIgnoreCase));
            options.CustomSchemaIds(type => type.FullName);
            options.OperationFilter<RemoveVersionParameterOperationFilter>();
            options.DocumentFilter<ReplaceVersionWithExactValueInPathDocumentFilter>();
        });

        return services;
    }

    public static WebApplication UseConfiguredSwagger(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Security Agency API {description.GroupName}");
        });

        return app;
    }
}

internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Security Agency API",
                Version = description.GroupName,
                Description = $"API for Security Agency Management System ({description.GroupName})"
            });
        }
    }
}

internal sealed class RemoveVersionParameterOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var versionParameter = operation.Parameters?.FirstOrDefault(parameter =>
            string.Equals(parameter.Name, "version", StringComparison.OrdinalIgnoreCase));

        if (versionParameter != null)
            operation.Parameters!.Remove(versionParameter);
    }
}

internal sealed class ReplaceVersionWithExactValueInPathDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var versionToken = "v{version}";
        var updatedPaths = new OpenApiPaths();

        foreach (var path in swaggerDoc.Paths)
        {
            updatedPaths.Add(
                path.Key.Replace(versionToken, swaggerDoc.Info.Version, StringComparison.OrdinalIgnoreCase),
                path.Value);
        }

        swaggerDoc.Paths = updatedPaths;
    }
}
