using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.SharedKernel.Exceptions;

namespace WarehouseManagement.Web.Handlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred");

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception),
            Detail = GetDetail(exception),
            Type = GetTypeUri(statusCode)
        };

        problemDetails.Extensions["code"] = GetErrorCode(exception);

        if (exception is DomainException domainException && domainException.Parameters.Length > 0)
        {
            var parameters = new Dictionary<string, object>();
            for (int i = 0; i < domainException.Parameters.Length; i++)
            {
                parameters[$"param{i}"] = domainException.Parameters[i];
            }
            problemDetails.Extensions["parameters"] = parameters;
        }

        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        EntityNotFoundException => StatusCodes.Status404NotFound,
        EntityInUseException => StatusCodes.Status409Conflict,
        DuplicateEntityException => StatusCodes.Status409Conflict,
        BusinessRuleValidationException => StatusCodes.Status400BadRequest,
        InsufficientBalanceException => StatusCodes.Status400BadRequest,
        SignedDocumentException => StatusCodes.Status400BadRequest,
        ArgumentNullException => StatusCodes.Status400BadRequest,
        System.ArgumentException => StatusCodes.Status400BadRequest,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(Exception exception) => exception switch
    {
        EntityNotFoundException => "Resource Not Found",
        EntityInUseException => "Resource In Use",
        DuplicateEntityException => "Duplicate Resource",
        BusinessRuleValidationException => "Business Rule Violation",
        InsufficientBalanceException => "Insufficient Balance",
        SignedDocumentException => "Signed Document Operation",
        ArgumentNullException => "Invalid Argument",
        System.ArgumentException => "Invalid Argument",
        InvalidOperationException => "Invalid Operation",
        _ => "Internal Server Error"
    };

    private string GetDetail(Exception exception)
    {
        if (exception is DomainException)
        {
            return exception.Message;
        }

        return exception switch
        {
            ArgumentNullException ex => $"Required parameter '{ex.ParamName}' is missing or null",
            System.ArgumentException ex => ex.Message,
            InvalidOperationException ex => ex.Message,
            _ => environment.IsDevelopment() 
                ? exception.Message 
                : "An unexpected error occurred. Please contact support if the problem persists."
        };
    }

    private static string GetErrorCode(Exception exception)
    {
        if (exception is DomainException domainException)
        {
            return domainException.Code;
        }

        return exception switch
        {
            ArgumentNullException => "INVALID_ARGUMENT",
            System.ArgumentException => "INVALID_ARGUMENT",
            InvalidOperationException => "INVALID_OPERATION",
            _ => "INTERNAL_SERVER_ERROR"
        };
    }

    private static string GetTypeUri(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            _ => $"https://httpstatuses.com/{statusCode}"
        };
    }
}
