using CSharpFunctionalExtensions;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel;
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
    public async Task<UnitResult<Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить локацию с id {LocationId}", location.Id);
            return Error.Internal("location.save.failed", "Не удалось сохранить локацию");
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

    public async Task<Result<Location, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var locationResult = await _dbContext.Locations
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (locationResult is null)
            return Error.NotFound("location.not.found", $"Локация с {id} не существует");

        return locationResult;
    }

    public async Task<UnitResult<Error>> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить данные в БД");
            return Error.Internal("not.save", "Не удалось сохранить данные в БД");
        }
    }
}