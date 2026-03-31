using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SecurityAgencyApp.API.Middleware;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Web.Services;

public class ApiClient : IApiClient
{
    private static readonly Regex VersionSegmentRegex = new(@"^v\d+(?:\.\d+)?(?:/|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<ApiClient> _logger;
    private readonly ApiClientOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(
        HttpClient http,
        IHttpContextAccessor httpContext,
        ILogger<ApiClient> logger,
        IOptions<ApiClientOptions> options)
    {
        _http = http;
        _httpContext = httpContext;
        _logger = logger;
        _options = options.Value;
    }

    private Uri BuildRequestUri(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
            return absoluteUri;

        var baseUri = GetBaseUri();
        var relativePath = NormalizeRelativePath(path);
        return new Uri(baseUri, relativePath);
    }

    private Uri GetBaseUri()
    {
        if (Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var configuredBaseUri))
            return EnsureTrailingSlash(configuredBaseUri);

        var ctx = _httpContext.HttpContext;
        if (ctx != null)
        {
            var request = ctx.Request;
            return new Uri($"{request.Scheme}://{request.Host}{request.PathBase}/");
        }

        if (_http.BaseAddress != null)
            return EnsureTrailingSlash(_http.BaseAddress);

        throw new InvalidOperationException("API base URL is not configured.");
    }

    private static Uri EnsureTrailingSlash(Uri uri)
    {
        var value = uri.ToString();
        return value.EndsWith("/", StringComparison.Ordinal) ? uri : new Uri(value + "/", UriKind.Absolute);
    }

    private string NormalizeRelativePath(string path)
    {
        var trimmedPath = path.Trim().TrimStart('/');
        if (string.IsNullOrEmpty(trimmedPath))
            return ComposeVersionedPath(string.Empty);

        var queryStartIndex = trimmedPath.IndexOf('?');
        var pathOnly = queryStartIndex >= 0 ? trimmedPath[..queryStartIndex] : trimmedPath;
        var queryString = queryStartIndex >= 0 ? trimmedPath[queryStartIndex..] : string.Empty;

        var routePrefix = _options.RoutePrefix.Trim('/');
        if (!string.IsNullOrEmpty(routePrefix) &&
            pathOnly.StartsWith(routePrefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            pathOnly = pathOnly[(routePrefix.Length + 1)..];
        }

        pathOnly = VersionSegmentRegex.Replace(pathOnly, string.Empty, 1);

        return ComposeVersionedPath(pathOnly) + queryString;
    }

    private string ComposeVersionedPath(string path)
    {
        var segments = new[]
        {
            _options.RoutePrefix.Trim('/'),
            NormalizeVersionSegment(_options.DefaultVersion),
            path.Trim('/')
        };

        return string.Join("/", segments.Where(segment => !string.IsNullOrWhiteSpace(segment)));
    }

    private static string NormalizeVersionSegment(string version)
    {
        var trimmed = version.Trim().Trim('/');
        if (string.IsNullOrEmpty(trimmed))
            return "v1";

        return trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"v{trimmed}";
    }

    private void SetHeaders(HttpRequestMessage request)
    {
        var ctx = _httpContext.HttpContext;
        if (ctx == null)
            return;

        var token = ctx.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tenantId = ctx.Session.GetString("TenantId");
        if (!string.IsNullOrEmpty(tenantId))
            request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId);

        var correlationId =
            ctx.Items[CorrelationIdMiddleware.CorrelationIdItemKey]?.ToString() ??
            ctx.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader].FirstOrDefault() ??
            ctx.TraceIdentifier;

        if (!string.IsNullOrWhiteSpace(correlationId) &&
            !request.Headers.Contains(CorrelationIdMiddleware.CorrelationIdHeader))
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.CorrelationIdHeader, correlationId);
        }
    }

    private void LogRequestStart(HttpMethod method, Uri requestUri)
    {
        if (!_options.EnableRequestLogging)
            return;

        _logger.LogInformation("API {Method} {Uri}", method.Method, requestUri);
    }

    private void LogRequestEnd(HttpMethod method, Uri requestUri, HttpResponseMessage response)
    {
        if (!_options.EnableRequestLogging)
            return;

        var correlationId = response.Headers.TryGetValues(CorrelationIdMiddleware.CorrelationIdHeader, out var values)
            ? values.FirstOrDefault()
            : null;

        _logger.LogInformation(
            "API {Method} {Uri} -> {StatusCode} {CorrelationId}",
            method.Method,
            requestUri,
            (int)response.StatusCode,
            correlationId ?? string.Empty);
    }

    public async Task<ApiResult<T?>> GetAsync<T>(string path, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var queryString = query != null && query.Count > 0
            ? "?" + string.Join("&", query.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"))
            : string.Empty;
        var requestUri = BuildRequestUri(path + queryString);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            SetHeaders(request);
            LogRequestStart(HttpMethod.Get, requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            LogRequestEnd(HttpMethod.Get, requestUri, response);
            return await ParseResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API GET {Uri} failed", requestUri);
            return new ApiResult<T?> { Success = false, Message = "API request failed." };
        }
    }

    public async Task<ApiResult<T?>> PostAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildRequestUri(path);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            SetHeaders(request);
            if (body != null)
                request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

            LogRequestStart(HttpMethod.Post, requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            LogRequestEnd(HttpMethod.Post, requestUri, response);
            return await ParseResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API POST {Uri} failed", requestUri);
            return new ApiResult<T?> { Success = false, Message = "API request failed. " + ex.Message };
        }
    }

    public async Task<ApiResult<T?>> PostMultipartAsync<T>(string path, IDictionary<string, object> formData, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildRequestUri(path);

        try
        {
            using var content = new MultipartFormDataContent();
            foreach (var kv in formData)
            {
                if (kv.Value is string s)
                    content.Add(new StringContent(s), kv.Key);
                else if (kv.Value is MultipartFile file)
                {
                    var streamContent = new StreamContent(file.Content);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, kv.Key, file.FileName);
                }
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            SetHeaders(request);
            request.Content = content;

            LogRequestStart(HttpMethod.Post, requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            LogRequestEnd(HttpMethod.Post, requestUri, response);
            return await ParseResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API POST multipart {Uri} failed", requestUri);
            return new ApiResult<T?> { Success = false, Message = "API request failed. " + ex.Message };
        }
    }

    public async Task<ApiResult<T?>> PutAsync<T>(string path, object body, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildRequestUri(path);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            SetHeaders(request);
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

            LogRequestStart(HttpMethod.Put, requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            LogRequestEnd(HttpMethod.Put, requestUri, response);
            return await ParseResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API PUT {Uri} failed", requestUri);
            return new ApiResult<T?> { Success = false, Message = "API request failed." };
        }
    }

    public async Task<ApiResult<T?>> PatchAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildRequestUri(path);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Patch, requestUri);
            SetHeaders(request);
            if (body != null)
                request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

            LogRequestStart(HttpMethod.Patch, requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            LogRequestEnd(HttpMethod.Patch, requestUri, response);
            return await ParseResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API PATCH {Uri} failed", requestUri);
            return new ApiResult<T?> { Success = false, Message = "API request failed." };
        }
    }

    public async Task<ApiResult<bool>> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildRequestUri(path);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            SetHeaders(request);

            LogRequestStart(HttpMethod.Delete, requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            LogRequestEnd(HttpMethod.Delete, requestUri, response);
            var parsed = await ParseResponse<object>(response, cancellationToken);

            return new ApiResult<bool>
            {
                Success = parsed.Success,
                Message = parsed.Message,
                Data = parsed.Success,
                Errors = parsed.Errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API DELETE {Uri} failed", requestUri);
            return new ApiResult<bool> { Success = false, Message = "API request failed." };
        }
    }

    public async Task<ApiResult<DownloadedFile?>> GetFileAsync(string path, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var queryString = query != null && query.Count > 0
            ? "?" + string.Join("&", query.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"))
            : string.Empty;
        var requestUri = BuildRequestUri(path + queryString);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            SetHeaders(request);

            LogRequestStart(HttpMethod.Get, requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            LogRequestEnd(HttpMethod.Get, requestUri, response);

            if (!response.IsSuccessStatusCode)
            {
                var message = response.ReasonPhrase ?? "Download failed";
                var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var json = await response.Content.ReadAsStringAsync(cancellationToken);
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("message", out var messageElement))
                            message = messageElement.GetString() ?? message;
                    }
                    catch
                    {
                    }
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound &&
                    message.Contains("Not Found", StringComparison.OrdinalIgnoreCase) &&
                    !message.Trim().Contains(" "))
                {
                    message = "Report endpoint not found. Deploy the updated API (with Report controller) to enable report generation.";
                }

                return new ApiResult<DownloadedFile?> { Success = false, Message = message };
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentTypeFile = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = "document";
            if (response.Content.Headers.ContentDisposition?.FileNameStar != null)
                fileName = response.Content.Headers.ContentDisposition.FileNameStar.Trim('"');
            else if (response.Content.Headers.ContentDisposition?.FileName != null)
                fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');

            return new ApiResult<DownloadedFile?>
            {
                Success = true,
                Data = new DownloadedFile { Content = bytes, ContentType = contentTypeFile, FileName = fileName }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API GET file {Uri} failed", requestUri);
            return new ApiResult<DownloadedFile?> { Success = false, Message = "Download failed. " + ex.Message };
        }
    }

    private static async Task<ApiResult<T?>> ParseResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var success = root.TryGetProperty("success", out var successElement) && successElement.GetBoolean();
            var message = root.TryGetProperty("message", out var messageElement) ? messageElement.GetString() ?? string.Empty : string.Empty;
            var errors = new List<ApiError>();

            if (root.TryGetProperty("errors", out var errorArray) && errorArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var error in errorArray.EnumerateArray())
                {
                    if (error.ValueKind == JsonValueKind.String)
                    {
                        errors.Add(ApiError.Create(error.GetString() ?? string.Empty));
                        continue;
                    }

                    if (error.ValueKind != JsonValueKind.Object)
                        continue;

                    var field = error.TryGetProperty("field", out var fieldElement) && fieldElement.ValueKind != JsonValueKind.Null
                        ? fieldElement.GetString()
                        : null;
                    var errorMessage = error.TryGetProperty("message", out var errorMessageElement) && errorMessageElement.ValueKind != JsonValueKind.Null
                        ? errorMessageElement.GetString()
                        : null;

                    if (!string.IsNullOrWhiteSpace(errorMessage))
                        errors.Add(ApiError.Create(errorMessage, field));
                }
            }

            T? data = default;
            if (root.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind != JsonValueKind.Null &&
                dataElement.ValueKind != JsonValueKind.Undefined)
            {
                data = JsonSerializer.Deserialize<T>(dataElement.GetRawText(), JsonOptions);
            }

            return new ApiResult<T?>
            {
                Success = success,
                Message = message,
                Data = data,
                Errors = errors.Count > 0 ? errors : null
            };
        }
        catch
        {
            return new ApiResult<T?>
            {
                Success = false,
                Message = response.IsSuccessStatusCode ? "Invalid response." : (json.Length > 200 ? json[..200] : json),
                Errors = null
            };
        }
    }
}
