using DirectoryService.Contracts;
using DirectoryService.Shared;
using DirectoryService.Web.Extensions;
using Serilog.Context;

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
        using (LogContext.PushProperty("RequestPath", context.Request.Path.ToString()))
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
                _logger.LogError(ex, "Необработанное исключение: {ExceptionMessage}", ex.Message);

                var failure = Error.Internal("internal.server.error", "Ошибка сервера").ToFailure();

                await WriteErrorResponseAsync(context, failure);
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, DomainException ex)
    {
        _logger.LogWarning(ex, "Доменное исключение: {ExceptionMessage}",ex.Message);

        var failure = ex.Error.ToFailure();

        await WriteErrorResponseAsync(context, failure);
    }

    private async static Task WriteErrorResponseAsync(HttpContext context, Failure failure)
    {
        context.Response.StatusCode = failure.ToStatusCode();

        await context.Response.WriteAsJsonAsync(Envelope.Fail<object>(failure));
    }
}