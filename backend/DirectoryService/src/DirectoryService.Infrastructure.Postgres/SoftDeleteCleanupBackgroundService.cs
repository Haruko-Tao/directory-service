using DirectoryService.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.Postgres;

public sealed class SoftDeleteCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SoftDeleteCleanupOptions _options;
    private readonly ILogger<SoftDeleteCleanupBackgroundService> _logger;

    public SoftDeleteCleanupBackgroundService(IServiceScopeFactory scopeFactory,
        IOptions<SoftDeleteCleanupOptions> options,
        ILogger<SoftDeleteCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Interval);

        _logger.LogInformation(
            "Сервис запущен с интервалом: {Interval}, RetentionPeriod: {RetentionPeriod}, BatchSize: {BatchSize}",
            _options.Interval, _options.RetentionPeriod, _options.BatchSize);
        
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var cleaner = scope.ServiceProvider.GetRequiredService<ISoftDeleteCleaner>();

                    var threshold = DateTime.UtcNow - _options.RetentionPeriod;

                    var deletedCount = await cleaner.CleanupAsync(threshold, _options.BatchSize, stoppingToken);

                    _logger.LogInformation("Успешно удалены строки в количестве: {DeletedCount} за порог времени: {Threshold}", deletedCount, threshold);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Прогон не удался, приложение работает, следующий будет по расписанию");
                }
            }
        }
        catch (OperationCanceledException)
        {
            #pragma warning disable S6667
            _logger.LogInformation("Очистка остановлена вместе с приложением");
            #pragma warning restore S6667
        }
    }
}