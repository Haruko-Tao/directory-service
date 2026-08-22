using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations;

public class Location
{
    public Guid Id { get; private set; }
    
    public Name Name { get; private set; }
    
    public Address Address { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }

    //EF Core
    private Location()
    {
        Name = null!;
        Address = null!;
    }
    
    private Location(Guid id, Name name, Address address, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Address = address;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Location, Error> Create(Name name, Address address)
    {
        var location = new Location(Guid.NewGuid(), name, address, DateTime.UtcNow, DateTime.UtcNow);

        return location;
    }

    public void Update(Name name, Address address)
    {
        Name = name;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }
}