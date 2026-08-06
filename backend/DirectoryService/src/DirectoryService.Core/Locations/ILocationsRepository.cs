using DirectoryService.Domain.Locations;

namespace DirectoryService.Core.Locations;

public interface ILocationsRepository
{
    Task AddAsync(Location location, CancellationToken cancellationToken);

    Task<bool> IsNameTakenAsync(string name, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}