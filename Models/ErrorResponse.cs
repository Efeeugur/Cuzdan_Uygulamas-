namespace Cüzdan_Uygulaması.Models;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public object? Details { get; set; }

    public ErrorResponse()
    {
    }

    public ErrorResponse(string message, string errorCode, int statusCode)
    {
        Message = message;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public ErrorResponse(string message, string errorCode, int statusCode, string traceId)
        : this(message, errorCode, statusCode)
    {
        TraceId = traceId;
    }
}