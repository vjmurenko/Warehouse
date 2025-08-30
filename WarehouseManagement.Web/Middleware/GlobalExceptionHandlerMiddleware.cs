using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.Web.Middleware;

/// <summary>
/// Global exception handling middleware that converts exceptions to standardized error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception, _environment);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment environment)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            EntityNotFoundException ex => new ErrorResponse(
                ex.Code,
                ex.Message,
                parameters: CreateParametersDictionary(ex.Parameters)),

            EntityInUseException ex => new ErrorResponse(
                ex.Code,
                ex.Message,
                parameters: CreateParametersDictionary(ex.Parameters)),

            DuplicateEntityException ex => new ErrorResponse(
                ex.Code,
                ex.Message,
                parameters: CreateParametersDictionary(ex.Parameters)),

            BusinessRuleValidationException ex => new ErrorResponse(
                ex.Code,
                ex.Message,
                parameters: CreateParametersDictionary(ex.Parameters)),

            InsufficientBalanceException ex => new ErrorResponse(
                ex.Code,
                ex.Message,
                parameters: CreateParametersDictionary(ex.Parameters)),

            SignedDocumentException ex => new ErrorResponse(
                ex.Code,
                ex.Message,
                parameters: CreateParametersDictionary(ex.Parameters)),

            ArgumentException ex when ex is not ArgumentNullException => new ErrorResponse(
                "INVALID_ARGUMENT",
                ex.Message),

            ArgumentNullException ex => new ErrorResponse(
                "INVALID_ARGUMENT",
                $"Required parameter '{ex.ParamName}' is missing or null"),

            InvalidOperationException ex => new ErrorResponse(
                "INVALID_OPERATION",
                ex.Message),

            _ => new ErrorResponse(
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred. Please contact support if the problem persists.",
                environment.IsDevelopment() ? exception.Message : null)
        };

        response.StatusCode = GetStatusCode(exception);
        errorResponse.TraceId = context.TraceIdentifier;

        // Add additional debug information in development
        if (environment.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        EntityNotFoundException => (int)HttpStatusCode.NotFound,
        EntityInUseException => (int)HttpStatusCode.Conflict,
        DuplicateEntityException => (int)HttpStatusCode.Conflict,
        BusinessRuleValidationException => (int)HttpStatusCode.BadRequest,
        InsufficientBalanceException => (int)HttpStatusCode.BadRequest,
        SignedDocumentException => (int)HttpStatusCode.BadRequest,
        ArgumentNullException => (int)HttpStatusCode.BadRequest,
        ArgumentException => (int)HttpStatusCode.BadRequest,
        InvalidOperationException => (int)HttpStatusCode.BadRequest,
        _ => (int)HttpStatusCode.InternalServerError
    };

    private static Dictionary<string, object>? CreateParametersDictionary(object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return null;

        var dict = new Dictionary<string, object>();
        for (int i = 0; i < parameters.Length; i++)
        {
            dict[$"param{i}"] = parameters[i];
        }
        return dict;
    }
}