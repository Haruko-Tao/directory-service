using DirectoryService.Domain.Departments;

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

    public static Result<DepartmentPosition> Create(Guid departmentId, Guid positionId)
    {
        if (departmentId == Guid.Empty)
            return Result<DepartmentPosition>.Failure("DepartmentId не может быть пустым!");
        
        if (positionId == Guid.Empty)
            return Result<DepartmentPosition>.Failure("PositionId не может быть пустым!");

        var departmentPosition = new DepartmentPosition(Guid.NewGuid(), departmentId, positionId);

        return Result<DepartmentPosition>.Success(departmentPosition);
    }
}