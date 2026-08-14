using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Core.Locations.Extensions;

public static class LocationExtensions
{
    public static LocationResponse ToResponse(this Location location)
    {
        return new LocationResponse(location.Id, location.Name.Value, location.Address.ToDto(), location.CreatedAt, location.UpdatedAt);
    }

    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(address.City, address.Street, address.House, address.Apartment);
    }
}