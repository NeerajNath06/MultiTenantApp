using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using SecurityAgencyApp.Web.Services;

namespace SecurityAgencyApp.Web.Extensions;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddConfiguredApiVersioning(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultVersion = ParseConfiguredVersion(configuration.GetValue<string>($"{ApiClientOptions.SectionName}:{nameof(ApiClientOptions.DefaultVersion)}"));

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = defaultVersion;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();

            foreach (var controller in GetVersionedApiControllers())
                options.Conventions.Controller(controller.ControllerType).HasApiVersion(controller.Version);
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new NamespaceVersionedApiRouteConvention());
        });

        return services;
    }

    private static ApiVersion ParseConfiguredVersion(string? version)
    {
        var normalized = version?.Trim().Trim('/').TrimStart('v', 'V');
        if (string.IsNullOrWhiteSpace(normalized))
            return new ApiVersion(1, 0);

        var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var major = parts.Length > 0 && int.TryParse(parts[0], out var parsedMajor) ? parsedMajor : 1;
        var minor = parts.Length > 1 && int.TryParse(parts[1], out var parsedMinor) ? parsedMinor : 0;
        return new ApiVersion(major, minor);
    }

    private static IEnumerable<(Type ControllerType, ApiVersion Version)> GetVersionedApiControllers()
    {
        var assembly = typeof(Program).Assembly;
        foreach (var controllerType in assembly.GetTypes())
        {
            if (!typeof(ControllerBase).IsAssignableFrom(controllerType) || controllerType.IsAbstract)
                continue;

            if (TryResolveVersion(controllerType.Namespace, out var version))
                yield return (controllerType, version);
        }
    }

    private static bool TryResolveVersion(string? controllerNamespace, out ApiVersion version)
    {
        version = default!;
        if (string.IsNullOrWhiteSpace(controllerNamespace))
            return false;

        var match = Regex.Match(controllerNamespace, @"\.v(?<major>\d+)(?:_(?<minor>\d+))?$", RegexOptions.IgnoreCase);
        if (!match.Success)
            return false;

        var major = int.Parse(match.Groups["major"].Value);
        var minor = match.Groups["minor"].Success ? int.Parse(match.Groups["minor"].Value) : 0;
        version = new ApiVersion(major, minor);
        return true;
    }

    private sealed class NamespaceVersionedApiRouteConvention : IApplicationModelConvention
    {
        private static readonly Regex HardCodedVersionPrefixRegex = new(@"^api/v\d+(?:\.\d+)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (!TryResolveVersion(controller.ControllerType.Namespace, out _))
                    continue;

                foreach (var selector in controller.Selectors.Where(selector => selector.AttributeRouteModel != null))
                {
                    var routeModel = selector.AttributeRouteModel!;
                    if (string.IsNullOrWhiteSpace(routeModel.Template))
                        continue;

                    routeModel.Template = HardCodedVersionPrefixRegex.Replace(routeModel.Template, "api/v{version:apiVersion}/", 1);
                }
            }
        }
    }
}
