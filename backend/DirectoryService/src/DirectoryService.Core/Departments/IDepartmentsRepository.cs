using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Core.Departments;

public interface IDepartmentsRepository
{
    Task AddAsync(Department department, CancellationToken cancellationToken);
    Task<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Department>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<bool> IsSlugTakenAsync(Slug slug, Guid? parentId, CancellationToken cancellationToken);

    Task<int> CountChildrenAsync(Guid departmentId, CancellationToken cancellationToken);
    Task<int> CountLocationLinksForDepartmentAsync(Guid departmentId, CancellationToken cancellationToken);
    Task<int> CountPositionLinksForDepartmentAsync(Guid departmentId, CancellationToken cancellationToken);

    Task RemoveAsync(Department department, CancellationToken cancellationToken);
    
    //связь отдел - локация
    Task AddDepartmentLocationAsync(DepartmentLocation departmentLocation, CancellationToken cancellationToken);
    Task<bool> ExistsDepartmentLocationAsync(Guid locationId, Guid departmentId, CancellationToken cancellationToken);

    Task<int> CountLinksForLocationAsync(Guid locationId, CancellationToken cancellationToken);
    Task<UnitResult<Error>> RemoveDepartmentLocationAsync(Guid locationId, Guid departmentId, CancellationToken cancellationToken);
    
    //связь отдел - позиция

    Task AddDepartmentPositionAsync(DepartmentPosition departmentPosition, CancellationToken cancellationToken);
    Task<bool> ExistsDepartmentPositionAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken);
    Task<UnitResult<Error>> RemoveDepartmentPositionAsync(Guid positionId, Guid departmentId, CancellationToken cancellationToken);
    Task<int> CountLinksForPositionAsync(Guid positionId, CancellationToken cancellationToken);

}