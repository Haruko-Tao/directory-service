using DirectoryService.Core.Locations;
using DirectoryService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres;

public static class InfrasturctureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var useDapper = configuration.GetValue<bool>("UseDapperRepository");

        if (useDapper)
        {
            services.AddScoped<ILocationsRepository, DapperLocationsRepository>();
        }
        else
        { 
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ILocationsRepository, EfLocationsRepository>();
        }

        return services;
    }
}