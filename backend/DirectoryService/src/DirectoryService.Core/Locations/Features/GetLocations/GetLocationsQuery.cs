using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.GetLocations;

public record GetLocationsQuery(int Page, int PageSize) : IQuery;
