using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.CreateDepartment;

public record CreateDepartmentCommand(string Name, string Slug, Guid? ParentId, IReadOnlyCollection<Guid> LocationIds) : ICommand;