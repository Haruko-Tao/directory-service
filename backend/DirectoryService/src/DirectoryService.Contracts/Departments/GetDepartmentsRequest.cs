namespace DirectoryService.Contracts.Departments;

public sealed record GetDepartmentsRequest(
    string? Search,
    string? SortBy,
    string? SortDir,
    int? Page,
    int? PageSize);