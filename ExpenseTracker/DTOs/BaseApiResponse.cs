namespace ExpenseTracker.DTOs;

public class BaseApiResponse<T>
{
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
    public T? Details { get; set; }

    public BaseApiResponse(int statusCode, string? errorMessage = null, string? message = null, T? details = default)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        Message = message;
        Details = details;
    }
}