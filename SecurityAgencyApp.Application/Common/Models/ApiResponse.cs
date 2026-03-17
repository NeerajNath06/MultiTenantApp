using System.Text.Json.Serialization;

namespace SecurityAgencyApp.Application.Common.Models;

public class ApiError
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ApiError Create(string message, string? field = null)
    {
        return new ApiError
        {
            Field = field,
            Message = message
        };
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ApiError>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, IEnumerable<ApiError>? errors = null)
    {
        var errorList = errors?.ToList() ?? new List<ApiError> { ApiError.Create(message) };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errorList
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, IEnumerable<string>? errors)
    {
        return ErrorResponse(message, errors?.Select(error => ApiError.Create(error)));
    }
}
