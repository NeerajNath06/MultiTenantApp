namespace SecurityAgencyApp.Web.Services;

/// <summary>
/// Lightweight API client for Web to call SecurityAgencyApp.API.
/// Web stays thin: no MediatR/DbContext in controllers, all data via API.
/// </summary>
public interface IApiClient
{
    Task<ApiResult<T?>> GetAsync<T>(string path, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T?>> PostAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default);
    /// <summary>Post multipart form data (e.g. file upload). formData: form field name -> string value or MultipartFile.</summary>
    Task<ApiResult<T?>> PostMultipartAsync<T>(string path, IDictionary<string, object> formData, CancellationToken cancellationToken = default);
    Task<ApiResult<T?>> PutAsync<T>(string path, object body, CancellationToken cancellationToken = default);
    Task<ApiResult<T?>> PatchAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> DeleteAsync(string path, CancellationToken cancellationToken = default);
    /// <summary>GET and return file bytes (e.g. document download, export). Returns Content, ContentType, FileName.</summary>
    Task<ApiResult<DownloadedFile?>> GetFileAsync(string path, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);
}

public class DownloadedFile
{
    public byte[] Content { get; set; } = null!;
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "download";
}

public class MultipartFile
{
    public Stream Content { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
