using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace GlobalStable.API.Middlewares;

public static class FluentResultExtension
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller, Func<object, IActionResult>? successCode = null)
    {
        if (result.IsSuccess)
        {
            if (result.Value is null)
            {
                return controller.NoContent();
            }

            return successCode is null
                ? controller.Ok(result.Value)
                : successCode(result.Value);
        }

        var firstError = result.Errors.FirstOrDefault();
        var statusCode = firstError?.Metadata.TryGetValue("StatusCode", out var code) == true && code is int sc
            ? sc
            : StatusCodes.Status400BadRequest;

        return controller.StatusCode(statusCode, new { Errors = result.Errors.Select(e => e.Message) });
    }
}