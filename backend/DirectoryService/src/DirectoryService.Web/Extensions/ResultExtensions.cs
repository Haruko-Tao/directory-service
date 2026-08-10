using DirectoryService.SharedKernel;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Web.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Failure failure)
    {
        var statusCode = failure.First().Type switch
        {
            ErrorType.CONFLICT => StatusCodes.Status409Conflict,
            ErrorType.NOTFOUND => StatusCodes.Status404NotFound,
            ErrorType.VALIDATION => StatusCodes.Status400BadRequest,
            ErrorType.INTERNAL => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(failure) { StatusCode = statusCode };
    }
}