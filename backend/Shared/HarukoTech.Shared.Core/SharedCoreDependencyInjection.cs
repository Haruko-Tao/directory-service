

using System.Reflection;
using HarukoTech.Shared.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HarukoTech.Shared.Core;

public static class SharedCoreDependencyInjection
{
    public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableToAny(
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>),
                typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}