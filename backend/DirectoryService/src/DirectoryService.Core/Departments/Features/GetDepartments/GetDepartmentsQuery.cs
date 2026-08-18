using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.GetDepartments;

public record GetDepartmentsQuery(int Page, int PageSize) : IQuery;