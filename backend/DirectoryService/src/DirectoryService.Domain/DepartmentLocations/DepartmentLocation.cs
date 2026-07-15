using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.DepartmentLocations;

public class DepartmentLocation
{
    public Guid Id { get; private set; }
    
    public Guid DepartmentId { get; private set; }
    
    public Guid LocationId { get; private set; }
    
    public bool IsPrimary { get; private set; }

    private DepartmentLocation(Guid id, Guid departmentId, Guid locationId, bool isPrimary)
    {
        Id = id;
        DepartmentId = departmentId;
        LocationId = locationId;
        IsPrimary = isPrimary;
    }

    public static Result<DepartmentLocation> Create(Guid departmentId, Guid locationId, bool isPrimary = false)
    {
        if (departmentId == Guid.Empty)
            return Result<DepartmentLocation>.Failure("DepartmentId не может быть пустым!");
        
        if (locationId == Guid.Empty)
            return Result<DepartmentLocation>.Failure("LocationId не может быть пустым!");

        var departmentLocation = new DepartmentLocation(Guid.NewGuid(), departmentId, locationId, isPrimary);

        return Result<DepartmentLocation>.Success(departmentLocation);
    }
}