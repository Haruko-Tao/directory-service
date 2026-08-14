using System.Text.Json.Serialization;
using DirectoryService.Contracts;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DirectoryService.Web;

public static class WebDependencyInjection
{
    public static IServiceCollection AddWebErrorHandling(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var failure = new Failure(context.ModelState.SelectMany(
                        entry =>
                            entry.Value?.Errors ?? Enumerable.Empty<ModelError>())
                    .Select(error => Error.Validation("request.invalid", error.ErrorMessage)));

                var envelope = Envelope.Fail<object>(failure);

                return new ObjectResult(envelope) { StatusCode = StatusCodes.Status400BadRequest };
            };
        });

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}