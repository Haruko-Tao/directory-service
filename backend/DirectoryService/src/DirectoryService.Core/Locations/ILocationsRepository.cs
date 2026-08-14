using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel;

namespace DirectoryService.Core.Locations;

public interface ILocationsRepository
{
    Task<UnitResult<Error>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<bool> IsNameTakenAsync(string name, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<Location, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Location>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
}