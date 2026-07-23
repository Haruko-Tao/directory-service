namespace DirectoryService.Contracts.Locations;

public record LocationResponse(Guid Id, string Name, AddressDto Address, DateTime CreatedAt, DateTime UpdatedAt);