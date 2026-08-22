using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.UpdateDepartment;

public record UpdateDepartmentCommand(Guid Id, string Name) : ICommand;