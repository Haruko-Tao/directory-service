using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.RemovePosition;

public record RemovePositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;