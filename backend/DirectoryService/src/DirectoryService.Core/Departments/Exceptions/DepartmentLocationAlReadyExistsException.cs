using DirectoryService.SharedKernel;

namespace DirectoryService.Core.Departments.Exceptions;

public class DepartmentLocationAlReadyExistsException : DomainException
{
    public DepartmentLocationAlReadyExistsException(Guid departmentId, Guid locationId) : base(Error.Conflict("exist.department.location",
        $"Связь с {departmentId} и {locationId} уже существует"))

    {

    }
}