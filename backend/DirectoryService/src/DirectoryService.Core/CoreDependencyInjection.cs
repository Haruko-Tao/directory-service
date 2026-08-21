using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Locations.Features.CreateLocation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Core;

public static class CoreDependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateLocationsValidator>();

        services.Scan(scan => scan.FromAssemblies(typeof(CoreDependencyInjection).Assembly)
            .AddClasses(classes => classes.AssignableToAny(
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>),
                typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}