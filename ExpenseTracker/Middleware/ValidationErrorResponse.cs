using System.Text.Json;
using ExpenseTracker.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Middleware;

public static class ValidationErrorResponse
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .GroupBy(entry => ToFieldName(entry.Key))
            .ToDictionary(
                group => group.Key,
                group => group
                    .SelectMany(entry => entry.Value!.Errors)
                    .Select(error => error.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(new BaseResponse<IDictionary<string, string[]>>
        {
            Status = StatusCodes.Status400BadRequest,
            Error = ErrorCodes.ValidationError,
            Message = ValidationException.DefaultMessage,
            Details = errors
        });
    }
    
    private static string ToFieldName(string key)
    {
        if (key.StartsWith("$."))
        {
            key = key[2..];
        }

        return string.Join('.', key.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
    }
}
