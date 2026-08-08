using DirectoryService.SharedKernel;

namespace DirectoryService.Core.Departments.Exceptions;

public class DepartmentNotFoundException : DomainException
{
    public DepartmentNotFoundException(Guid id) : base(
        Error.NotFound("department.not.found", $"Отдел с {id} не найден"))
    {

    }
}