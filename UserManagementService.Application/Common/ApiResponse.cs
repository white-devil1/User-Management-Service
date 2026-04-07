namespace UserManagementService.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T? data, string message = "Success")
        => new() { Success = true, StatusCode = 200, Message = message, Data = data };

    public static ApiResponse<T> Created(T data, string message = "Created successfully")
        => new() { Success = true, StatusCode = 201, Message = message, Data = data };

    public static ApiResponse<T> Fail(
        int statusCode, string message, List<string>? errors = null)
        => new()
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? new List<string>()
        };
}

// Non-generic version for responses with no data (delete, toggle etc)
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Success")
        => new() { Success = true, StatusCode = 200, Message = message };

    public static new ApiResponse Fail(
        int statusCode, string message, List<string>? errors = null)
        => new()
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? new List<string>()
        };
}
