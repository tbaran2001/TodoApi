using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Todo.Api.Common.Exceptions;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        if (exception is ValidationException validationException)
        {
            await HandleValidationExceptionAsync(context, validationException, cancellationToken);
            return true;
        }

        var (detail, title, statusCode) = exception switch
        {
            BadHttpRequestException ex => (
                ex.InnerException is JsonException jsonEx
                    ? $"Invalid value provided for path: '{jsonEx.Path}'. Please check the data type and format."
                    : "The request body contains malformed JSON.",
                "Bad Request",
                StatusCodes.Status400BadRequest),
            NotFoundException ex => (ex.Message, "Not Found", StatusCodes.Status404NotFound),
            _ => ("An unexpected internal server error has occurred.", "Internal Server Error",
                StatusCodes.Status500InternalServerError)
        };

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = statusCode,
            Instance = context.Request.Path,
            Extensions = { { "traceId", context.TraceIdentifier } }
        }, cancellationToken);

        return true;
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception,
        CancellationToken cancellationToken)
    {
        var validationProblemDetails = new ValidationProblemDetails(
            exception.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
        {
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.Request.Path,
            Extensions = { { "traceId", context.TraceIdentifier } }
        };

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken: cancellationToken);
    }
}