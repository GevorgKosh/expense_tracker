using ExpenseTracker.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ExpenseTracker.Middleware;

public class ExpenseExceptionHandler(ILogger<ExpenseExceptionHandler> logger): IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            logger.LogError(exception, "Exception occurred after the response has started");
            return false;
        }

        (int status, string error, string message, object? details) = exception switch
        {
            ValidationException validation => (StatusCodes.Status400BadRequest, ErrorCodes.ValidationError, validation.Message, validation.Errors),
            BadRequestException   => (StatusCodes.Status400BadRequest,   ErrorCodes.BadRequest,   exception.Message, null),
            NotFoundException     => (StatusCodes.Status404NotFound,     ErrorCodes.NotFound,     exception.Message, null),
            ConflictException     => (StatusCodes.Status409Conflict,     ErrorCodes.Conflict,     exception.Message, null),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, ErrorCodes.Unauthorized, exception.Message, null),
            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalServerError, "An unexpected error occurred", null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning("Handled exception: {Message}", exception.Message);

        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(new BaseResponse<object>
        {
            Status = status,
            Error = error,
            Message = message,
            Details = details
        }, cancellationToken);

        return true;
    }
}