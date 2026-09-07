namespace DirectoryService.Contracts.Locations;

public sealed record LocationListItemDto(Guid Id, string Name, AddressDto Address, DateTime CreatedAt, int DepartmentCount);