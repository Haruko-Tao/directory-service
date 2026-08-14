using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    public Guid Id { get; private set; }
    
    public Guid DepartmentId { get; private set; }
    
    public Guid PositionId { get; private set; }

    private DepartmentPosition(Guid id, Guid departmentId, Guid positionId)
    {
        Id = id;
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public static Result<DepartmentPosition, Error> Create(Guid departmentId, Guid positionId)
    {
        if (departmentId == Guid.Empty)
            return Error.Validation("departmentid.not.empty", "Департамент должен существовать");

        if (positionId == Guid.Empty)
            return Error.Validation("validationid.not.empty", "Позиция должна существовать");

        var departmentPosition = new DepartmentPosition(Guid.NewGuid(), departmentId, positionId);

        return departmentPosition;
    }
}