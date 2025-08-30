namespace WarehouseManagement.Application.Common.Models;

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public string? StackTrace { get; set; }

    public ErrorResponse()
    {
    }

    public ErrorResponse(string code, string message, string? details = null, Dictionary<string, object>? parameters = null)
    {
        Code = code;
        Message = message;
        Details = details;
        Parameters = parameters;
    }
}

/// <summary>
/// Validation error response with field-specific errors
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ValidationErrorResponse() : base("VALIDATION_ERROR", "One or more validation errors occurred")
    {
    }

    public ValidationErrorResponse(Dictionary<string, string[]> errors) 
        : base("VALIDATION_ERROR", "One or more validation errors occurred")
    {
        Errors = errors;
    }
}