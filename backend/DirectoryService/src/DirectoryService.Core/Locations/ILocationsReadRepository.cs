using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations.Features.GetLocations;
using HarukoTech.Shared.Core;
using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Locations;

public interface ILocationsReadRepository
{
    Task<PagedResult<LocationListItemDto>> GetPageAsync(GetLocationsQuery query,
        CancellationToken cancellationToken);
}