using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand;