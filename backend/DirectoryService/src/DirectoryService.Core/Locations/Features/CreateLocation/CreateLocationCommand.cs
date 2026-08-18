using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.CreateLocation;

public record CreateLocationCommand(string Name, AddressDto AddressDto) : ICommand;