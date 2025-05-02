using System.Net;
using EventManager.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using FluentValidation;

namespace EventManager.API.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cst)
    {
        var (statusCode, message) = GetExceptionDetails(exception);
        
        _logger.LogError(exception, exception.Message);

        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(message, cst);

        return true;
    }

    private (HttpStatusCode statusCode, string message) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                (HttpStatusCode.BadRequest, GetValidationErrorMessage(validationException)),
            UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message),
            LoginFailedException => (HttpStatusCode.Unauthorized, exception.Message),
            UserAlreadyExistsException => (HttpStatusCode.Conflict, exception.Message),
            RegistrationFailedException => (HttpStatusCode.BadRequest, exception.Message),
            RefreshTokenException => (HttpStatusCode.Unauthorized, exception.Message),
            UserNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            UserAlreadyAdminException => (HttpStatusCode.Conflict, exception.Message),
            PromotionFailedException => (HttpStatusCode.BadRequest, exception.Message),
            FileStorageException => (HttpStatusCode.InternalServerError, exception.Message),
            OperationCanceledException or TaskCanceledException =>
            (HttpStatusCode.BadRequest, "Request was canceled."),
            _ => (HttpStatusCode.InternalServerError, $"An unexpected error occurred: {exception.Message}")
        };
    }

    private string GetValidationErrorMessage(ValidationException validationException)
    {
        var errors = validationException.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
        return string.Join("\n", errors);
    }
}