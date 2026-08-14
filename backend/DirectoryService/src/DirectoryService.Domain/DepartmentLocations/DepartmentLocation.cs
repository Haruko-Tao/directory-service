using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

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

    public static Result<DepartmentLocation, Error> Create(Guid departmentId, Guid locationId, bool isPrimary = false)
    {
        if (departmentId == Guid.Empty)
            return Error.Validation("departmentid.not.empty", "Департамент должен существовать");
        
        if (locationId == Guid.Empty)
            return Error.Validation("locationid.not.empty", "Локация должна существовать");

        var departmentLocation = new DepartmentLocation(Guid.NewGuid(), departmentId, locationId, isPrimary);

        return departmentLocation;
    }
}