using System.Collections;

namespace DirectoryService.Shared;

public class Failure : IEnumerable<Error>
{
    private readonly List<Error> _errors;

    public Failure(IEnumerable<Error> errors)
    {
        _errors = [..errors];
    }
    
    public IEnumerator<Error> GetEnumerator()
    {
        return _errors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static Failure None => new Failure([]);
    public static implicit operator Failure(Error[] errors) => new(errors);
    public static implicit operator Failure(Error error) => new([error]);
}