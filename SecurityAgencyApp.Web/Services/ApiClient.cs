using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SecurityAgencyApp.Web.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContext;
    private readonly ILogger<ApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(HttpClient http, IHttpContextAccessor httpContext, ILogger<ApiClient> logger)
    {
        _http = http;
        _httpContext = httpContext;
        _logger = logger;
    }

    /// <summary>Build absolute URI so request always hits API (BaseAddress + path with correct slash).</summary>
    private Uri BuildRequestUri(string path)
    {
        if (_http.BaseAddress == null)
            return new Uri(path.TrimStart('/'), UriKind.Relative);
        var segment = path.TrimStart('/');
        var baseUrl = _http.BaseAddress.ToString().TrimEnd('/');
        return new Uri(baseUrl + "/" + segment, UriKind.Absolute);
    }

    private void SetAuthHeaders(HttpRequestMessage request)
    {
        var ctx = _httpContext.HttpContext;
        if (ctx == null) return;

        var token = ctx.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tenantId = ctx.Session.GetString("TenantId");
        if (!string.IsNullOrEmpty(tenantId))
            request.Headers.TryAddWithoutValidation("X-Tenant-Id", tenantId);
    }

    public async Task<ApiResult<T?>> GetAsync<T>(string path, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var qs = query != null && query.Count > 0
            ? "?" + string.Join("&", query.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"))
            : "";
        var requestUri = BuildRequestUri(path + qs);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            SetAuthHeaders(request);
            var response = await _http.SendAsync(request, cancellationToken);
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
            SetAuthHeaders(request);
            if (body != null)
                request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            _logger.LogInformation("API POST {Uri}", requestUri);
            var response = await _http.SendAsync(request, cancellationToken);
            _logger.LogInformation("API POST {Uri} -> {StatusCode}", requestUri, response.StatusCode);
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
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, kv.Key, file.FileName);
                }
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            SetAuthHeaders(request);
            request.Content = content;
            var response = await _http.SendAsync(request, cancellationToken);
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
            SetAuthHeaders(request);
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(request, cancellationToken);
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
            SetAuthHeaders(request);
            if (body != null)
                request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(request, cancellationToken);
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
            SetAuthHeaders(request);
            var response = await _http.SendAsync(request, cancellationToken);
            var parsed = await ParseResponse<object>(response, cancellationToken);
            return new ApiResult<bool> { Success = parsed.Success, Message = parsed.Message, Data = parsed.Success };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API DELETE {Uri} failed", requestUri);
            return new ApiResult<bool> { Success = false, Message = "API request failed." };
        }
    }

    public async Task<ApiResult<DownloadedFile?>> GetFileAsync(string path, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default)
    {
        var qs = query != null && query.Count > 0
            ? "?" + string.Join("&", query.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"))
            : "";
        var requestUri = BuildRequestUri(path + qs);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            SetAuthHeaders(request);
            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new ApiResult<DownloadedFile?> { Success = false, Message = response.ReasonPhrase ?? "Download failed" };
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = "document";
            if (response.Content.Headers.ContentDisposition?.FileNameStar != null)
                fileName = response.Content.Headers.ContentDisposition.FileNameStar.Trim('"');
            else if (response.Content.Headers.ContentDisposition?.FileName != null)
                fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');
            return new ApiResult<DownloadedFile?>
            {
                Success = true,
                Data = new DownloadedFile { Content = bytes, ContentType = contentType, FileName = fileName }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API GET file {Uri} failed", requestUri);
            return new ApiResult<DownloadedFile?> { Success = false, Message = "Download failed." };
        }
    }

    private static async Task<ApiResult<T?>> ParseResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var success = root.TryGetProperty("success", out var s) && s.GetBoolean();
            var message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";
            T? data = default;
            if (root.TryGetProperty("data", out var d) && d.ValueKind != JsonValueKind.Null && d.ValueKind != JsonValueKind.Undefined)
                data = JsonSerializer.Deserialize<T>(d.GetRawText(), JsonOptions);
            return new ApiResult<T?>
            {
                Success = success,
                Message = message ?? "",
                Data = data
            };
        }
        catch
        {
            return new ApiResult<T?>
            {
                Success = false,
                Message = response.IsSuccessStatusCode ? "Invalid response." : (json.Length > 200 ? json.Substring(0, 200) : json)
            };
        }
    }
}
