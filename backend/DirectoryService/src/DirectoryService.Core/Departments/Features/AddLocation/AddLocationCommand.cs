using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.AddLocation;

public record AddLocationCommand(Guid DepartmentId, Guid LocationId) : ICommand;