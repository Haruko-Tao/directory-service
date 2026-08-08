using DirectoryService.SharedKernel;

namespace DirectoryService.Core.Locations.Exceptions;

public class LocationNameDuplicateException : DomainException
{
    public LocationNameDuplicateException(string name) : base(Error.Conflict("location.name.duplicate",
        $"Локация с {name} уже существует"))
    {
       
    }
}