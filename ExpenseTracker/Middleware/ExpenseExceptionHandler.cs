using ExpenseTracker.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;

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

        var (status, message) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            BadRequestException => (StatusCodes.Status400BadRequest, exception.Message),
            ConflictException   => (StatusCodes.Status409Conflict,   exception.Message),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
        
        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning("Handled exception: {Message}", exception.Message);

        httpContext.Response.StatusCode = status;
        
        await httpContext.Response.WriteAsJsonAsync(new BaseResponse<object>
        {
            Status = status,
            Error = ReasonPhrases.GetReasonPhrase(status),
            Message = message,
            Details = null
        }, cancellationToken);

        return true;
    }
}