namespace DirectoryService.Contracts.Locations;

public record TopLocationsResponse(Guid Id, string Name, AddressDto Address, int DepartmentCount);