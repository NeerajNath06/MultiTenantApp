namespace SecurityAgencyApp.Web.Services;

/// <summary>
/// Lightweight API client for Web to call SecurityAgencyApp.API.
/// Web stays thin: no MediatR/DbContext in controllers, all data via API.
/// </summary>
public interface IApiClient
{
    Task<ApiResult<T?>> GetAsync<T>(string path, IDictionary<string, string?>? query = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T?>> PostAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T?>> PutAsync<T>(string path, object body, CancellationToken cancellationToken = default);
    Task<ApiResult<T?>> PatchAsync<T>(string path, object? body = null, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> DeleteAsync(string path, CancellationToken cancellationToken = default);
}

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
