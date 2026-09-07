using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations.Features.GetLocations;

namespace DirectoryService.Core.Locations;

public interface ILocationsReadRepository
{
    Task<PagedResult<LocationListItemDto>> GetPageAsync(GetLocationsQuery query,
        CancellationToken cancellationToken);
}