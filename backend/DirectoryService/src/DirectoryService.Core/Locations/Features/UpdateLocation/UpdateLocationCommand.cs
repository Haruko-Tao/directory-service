using DirectoryService.Contracts.Locations;
using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Locations.Features.UpdateLocation;

public record UpdateLocationCommand(Guid LocationId,string Name, AddressDto AddressDto) : ICommand;
