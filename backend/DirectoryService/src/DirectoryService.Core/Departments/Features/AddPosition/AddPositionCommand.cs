using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.AddPosition;

public record AddPositionCommand(Guid DepartmentId, Guid PositionId) : ICommand;