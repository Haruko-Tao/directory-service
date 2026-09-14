using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;

namespace DirectoryService.Core.Locations;

public interface ILocationsRepository
{
    Task AddAsync(Location location, CancellationToken cancellationToken);

    Task<bool> IsNameTakenAsync(Name name, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<Location, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Location>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
}