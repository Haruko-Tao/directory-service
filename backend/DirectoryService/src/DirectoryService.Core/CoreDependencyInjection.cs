using DirectoryService.Core.Locations.Features.CreateLocation;
using FluentValidation;
using HarukoTech.Shared.Core;
using Microsoft.Extensions.DependencyInjection;


namespace DirectoryService.Core;

public static class CoreDependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateLocationsValidator>();

        services.AddHandlers(typeof(CoreDependencyInjection).Assembly);

        return services;
    }
}