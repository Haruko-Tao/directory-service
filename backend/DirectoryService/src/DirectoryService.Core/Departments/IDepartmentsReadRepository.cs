using DirectoryService.Contracts.Departments;

namespace DirectoryService.Core.Departments;

public interface IDepartmentsReadRepository
{
    Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetRootsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetChildrenAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetAncestorsAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetSearchAsync(string query, CancellationToken cancellationToken);
}