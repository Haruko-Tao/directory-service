using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Core.Departments;

public interface IDepartmentsRepository
{
    Task<UnitResult<Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<UnitResult<Error>> AddDepartmentLocationAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveAsync(CancellationToken cancellationToken);

    Task<bool> ExistsDepartmentLocationAsync(Guid locationId, Guid departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> RemoveDepartmentLocationAsync(Guid locationId, Guid departmentId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Department>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<bool> IsSlugTakenAsync(string slug, Guid? parentId, CancellationToken cancellationToken);
}