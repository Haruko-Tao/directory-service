using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Core.Departments.Extensions;

public static class DepartmentExtensions
{
    public static DepartmentResponse ToResponse(this Department department)
    {
        return new DepartmentResponse(department.Id, department.Name.Value, department.Slug.Value,
            department.Path.Value, department.ParentId, department.CreatedAt, department.UpdatedAt);
    }
}