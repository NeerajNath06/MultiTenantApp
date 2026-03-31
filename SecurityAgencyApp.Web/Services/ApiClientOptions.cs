namespace SecurityAgencyApp.Web.Services;

public class ApiClientOptions
{
    public const string SectionName = "ApiClient";

    public string BaseUrl { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = "api";
    public string DefaultVersion { get; set; } = "v1";
    public bool EnableRequestLogging { get; set; } = true;
}
