using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.UpdateDepartments;

public record UpdateDepartmentCommand(Guid Id,string Name) : ICommand;