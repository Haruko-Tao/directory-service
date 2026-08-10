using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel;

namespace DirectoryService.Domain.Positions;

public class Position
{
    public Guid Id { get; private set; }
    
    public Name Name { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }

    //EF Core
    private Position()
    {
        Name = null!;
    }
    
    private Position(Guid id, Name name, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Position, Error> Create(Name name)
    {

        var position = new Position(Guid.NewGuid(), name, DateTime.UtcNow, DateTime.UtcNow);

        return position;
    }
}