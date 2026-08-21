using CSharpFunctionalExtensions;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class EfLocationsRepository : ILocationsRepository
{
    private readonly AppDbContext _dbContext;

    public EfLocationsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);
        
    }

    public async Task<bool> IsNameTakenAsync(Name name, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations
            .AnyAsync(l => l.Name.Value == name.Value, cancellationToken);
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

    public async Task<IReadOnlyList<Location>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations
            .OrderBy(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}