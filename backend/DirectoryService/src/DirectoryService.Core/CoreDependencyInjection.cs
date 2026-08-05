using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Core;

public static class CoreDependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<LocationsService>();

        services.AddValidatorsFromAssemblyContaining<CreateLocationsValidator>();

        services.AddScoped<DepartmentsService>();

        return services;
    }
}