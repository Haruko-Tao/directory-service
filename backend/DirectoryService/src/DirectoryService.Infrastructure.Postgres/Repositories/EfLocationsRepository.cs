using DirectoryService.Core.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class EfLocationsRepository : ILocationsRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<EfLocationsRepository> _logger;

    public EfLocationsRepository(AppDbContext dbContext, ILogger<EfLocationsRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить локацию с id {LocationId}", location.Id);
            throw;
        }
    }

    public async Task<bool> IsNameTakenAsync(string name, CancellationToken cancellationToken)
    {
        var nameResult = Name.Create(name);
        
        return await _dbContext.Locations
            .AnyAsync(l => l.Name == nameResult.Value!, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations
            .AnyAsync(l => l.Id == id, cancellationToken);
    }
}