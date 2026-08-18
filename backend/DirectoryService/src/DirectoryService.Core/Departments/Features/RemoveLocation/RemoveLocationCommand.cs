using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.RemoveLocation;

public record RemoveLocationCommand(Guid LocationId, Guid DepartmentId) : ICommand;