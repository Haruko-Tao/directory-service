namespace DirectoryService.Contracts.Locations;

public record GetLocationsRequest(string? Search, string? SortBy, string? SortDir, int? Page, int? PageSize, int? MinDepartmentCount);