using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;

namespace DirectoryService.Core.Departments;

public interface IDepartmentsRepository
{
    Task AddAsync(Department department, CancellationToken cancellationToken);

    Task AddDepartmentLocationAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken);

    Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}