using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.GetLocations;

public record GetLocationsQuery(
    string? Search,
    string SortBy,
    string SortDir,
    int Page,
    int PageSize,
    int? MinDepartmentCount) : IQuery;
