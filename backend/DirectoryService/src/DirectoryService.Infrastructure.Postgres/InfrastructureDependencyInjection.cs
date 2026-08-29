using DirectoryService.Core.Database;
using DirectoryService.Core.Departments;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace DirectoryService.Infrastructure.Postgres;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Не задана строка подключения");
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IReadDbContext>(provider => new ReadDbContext(connectionString,
            provider.GetRequiredService<ILoggerFactory>()));
        
        services.AddScoped<ILocationsRepository, EfLocationsRepository>();
            
        services.AddScoped<IDepartmentsRepository, EfDepartmentsRepository>();

        services.AddScoped<IPositionsRepository, EfPositionsRepository>();

        services.AddScoped<ITransactionManager, TransactionManager>();
        
        

        return services;
    }
}