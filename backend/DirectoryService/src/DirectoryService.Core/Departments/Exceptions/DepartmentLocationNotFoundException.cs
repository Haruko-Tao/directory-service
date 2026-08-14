using DirectoryService.Shared;

namespace DirectoryService.Core.Departments.Exceptions;

public class DepartmentLocationNotFoundException : DomainException
{
    public DepartmentLocationNotFoundException(Guid departmentId, Guid locationId) : base(
        Error.NotFound("department.location.not.found", $"Связи между {departmentId} и {locationId} не существует"))
    {

    }
}