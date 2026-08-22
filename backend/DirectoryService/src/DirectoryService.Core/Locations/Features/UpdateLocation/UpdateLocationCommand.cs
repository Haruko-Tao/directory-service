using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.UpdateLocation;

public record UpdateLocationCommand(Guid LocationId,string Name, AddressDto AddressDto) : ICommand;
