using DirectoryService.SharedKernel;

namespace DirectoryService.Core.Fails;

public class ValidationFailException : DomainException
{
    public ValidationFailException(IEnumerable<string> errors) : base(Error.Validation("invalid", errors.ToArray()))
    {
    }
}