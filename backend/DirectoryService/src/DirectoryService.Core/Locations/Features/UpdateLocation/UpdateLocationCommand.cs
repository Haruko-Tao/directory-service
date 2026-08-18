using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.UpdateLocations;

public record UpdateLocationCommand(Guid LocationId,string Name, AddressDto AddressDto) : ICommand;
