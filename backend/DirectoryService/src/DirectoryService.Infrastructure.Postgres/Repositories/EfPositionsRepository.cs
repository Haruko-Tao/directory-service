using CSharpFunctionalExtensions;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class EfPositionsRepository : IPositionsRepository
{
    private readonly AppDbContext _dbContext;

    public EfPositionsRepository(AppDbContext dbContext) { _dbContext = dbContext; }
    
    public async Task AddAsync(Position position, CancellationToken cancellationToken)
    {
        await _dbContext.Positions.AddAsync(position, cancellationToken);
    }

    public async Task<bool> IsNameTakenAsync(Name name, CancellationToken cancellationToken)
    {
        return await _dbContext.Positions.AnyAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Positions.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Result<Position, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var positionResult = await _dbContext.Positions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (positionResult is null)
            return Error.NotFound("position.not.found", $"Позиция с таким {id} не существует");

        return positionResult;
    }

    public async Task<IReadOnlyList<Position>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _dbContext.Positions
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public Task RemoveAsync(Position position, CancellationToken cancellationToken)
    {
        _dbContext.Positions.Remove(position);
        
        return Task.CompletedTask;
    }
}