using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Positions.Features.DeletePosition;

public record DeletePositionCommand(Guid Id) : ICommand;