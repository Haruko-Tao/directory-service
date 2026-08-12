using CSharpFunctionalExtensions;
using DirectoryService.Contracts;
using DirectoryService.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.Web.Extensions;

public static class ResultExtensions
{
    public static int ToStatusCode(this Failure failure)
    {
        if (failure.Any(f => f.Type == ErrorType.INTERNAL))
            return  StatusCodes.Status500InternalServerError ;

        if (failure.Any(f => f.Type == ErrorType.CONFLICT))
            return StatusCodes.Status409Conflict;

        if (failure.Any(f => f.Type == ErrorType.NOTFOUND))
            return StatusCodes.Status404NotFound;

        if (failure.Any(f => f.Type == ErrorType.VALIDATION))
            return StatusCodes.Status400BadRequest;

        return StatusCodes.Status500InternalServerError;
    }
    
    public static IResult ToApiResult<T>(this Result<T, Failure> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsFailure)
        {
            var statusCode = result.Error.ToStatusCode();
            return Results.Json(Envelope.Fail<T>(result.Error), statusCode: statusCode);
        }

        return Results.Json(Envelope.Success<T>(result.Value), statusCode: successStatusCode);
    }

    public static IResult ToApiResult(this UnitResult<Failure> result, int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsFailure)
        {
            var statusCode = result.Error.ToStatusCode();
            return Results.Json(Envelope.Fail<object>(result.Error), statusCode: statusCode);
        }

        return Results.Json(Envelope.Success<object>(default!), statusCode: successStatusCode);
    }
}