using System.Net;
using Newtonsoft.Json;

namespace ExpenseTracker.Middleware;

public class ExpenseTrackerResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExpenseTrackerResponseMiddleware> _logger;
    
    public ExpenseTrackerResponseMiddleware(
        RequestDelegate next,
        ILogger<ExpenseTrackerResponseMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong: {ex}");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        context.Response.StatusCode = exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message
        };

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}