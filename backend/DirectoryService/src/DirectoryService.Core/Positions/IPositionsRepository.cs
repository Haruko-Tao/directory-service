using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;

namespace DirectoryService.Core.Positions;

public interface IPositionsRepository
{
    Task AddAsync(Position position,CancellationToken cancellationToken);

    Task<bool> IsNameTakenAsync(Name name, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<Position ,Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Position>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task RemoveAsync(Position position, CancellationToken cancellationToken);
}