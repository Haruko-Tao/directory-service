using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Locations;

public class Location
{
    public Guid Id { get; private set; }
    
    public Name Name { get; private set; }
    
    public Address Address { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }

    private Location(Guid id, Name name, Address address, DateTime creadetAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Address = address;
        CreatedAt = creadetAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Location> Create(Name name, Address address)
    {
        var location = new Location(Guid.NewGuid(), name, address, DateTime.UtcNow, DateTime.UtcNow);

        return Result<Location>.Success(location);
    }
}