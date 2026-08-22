using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Positions.Features.CreatePosition;

public record CreatePositionCommand(string Name) : ICommand;