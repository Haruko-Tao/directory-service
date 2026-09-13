using DirectoryService.Core;
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

        services.AddScoped<ILocationsReadRepository>(_ => new DapperLocationsRepository(connectionString));
        
        services.AddScoped<ILocationsRepository, EfLocationsRepository>();
            
        services.AddScoped<IDepartmentsRepository, EfDepartmentsRepository>();

        services.AddScoped<IPositionsRepository, EfPositionsRepository>();

        services.AddScoped<ITransactionManager, TransactionManager>();

        services.AddOptions<SoftDeleteCleanupOptions>()
            .Bind(configuration.GetSection(SoftDeleteCleanupOptions.SectionName))
            .Validate(options => 
                options.Interval > TimeSpan.Zero,
                "Интервал должен быть больше нуля")
            .Validate(options => 
                options.RetentionPeriod > TimeSpan.Zero, "Период очистки должен быть больше 0")
            .Validate(options => 
                options.BatchSize > 0, "Размер очистки должен быть больше 0")
            .ValidateOnStart();

        services.AddScoped<ISoftDeleteCleaner>(_ => new SoftDeleteCleaner(connectionString));
        
        services.AddHostedService<SoftDeleteCleanupBackgroundService>();

        return services;
    }
}