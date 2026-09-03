using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.GetDepartments;

public record GetDepartmentsQuery(
    string? Search,
    string SortBy,
    string SortDir,
    int Page,
    int PageSize) : IQuery;