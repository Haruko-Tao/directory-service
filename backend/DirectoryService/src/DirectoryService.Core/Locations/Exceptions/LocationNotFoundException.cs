using DirectoryService.SharedKernel;

namespace DirectoryService.Core.Locations.Exceptions;

public class LocationNotFoundException : DomainException
{
    public LocationNotFoundException(Guid id) : base(Error.NotFound("location.not.found",
        $"Локация с {id} не найдена"))
    {

    }
}