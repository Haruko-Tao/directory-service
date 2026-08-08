using DirectoryService.SharedKernel;

namespace DirectoryService.Web.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (DomainException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(Error.Internal(code: default,message: "Произошла ошибка на стороне сервера"));
        }
    }

    public async Task HandleExceptionAsync(HttpContext context, DomainException ex)
    {
        _logger.LogError(ex, ex.Message);

        var (statusCode, error) = ex.Error.Type switch
        {
            ErrorType.VALIDATION => (StatusCodes.Status400BadRequest, ex.Error),
            ErrorType.NOTFOUND => (StatusCodes.Status404NotFound, ex.Error),
            ErrorType.CONFLICT => (StatusCodes.Status409Conflict, ex.Error),
            ErrorType.INTERNAL => (StatusCodes.Status500InternalServerError, ex.Error),
            _ => (StatusCodes.Status500InternalServerError, ex.Error)
        };

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(error);
    }
}