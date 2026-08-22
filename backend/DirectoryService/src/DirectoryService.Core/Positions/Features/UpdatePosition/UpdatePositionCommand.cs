using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Positions.Features.UpdatePosition;

public record UpdatePositionCommand(Guid Id, string Name) : ICommand;