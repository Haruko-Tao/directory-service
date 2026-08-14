using DirectoryService.Shared;

namespace DirectoryService.Contracts;

public record Envelope<T>
{
    public T Data { get; }
    public Failure Errors { get; }

    internal Envelope(T data, Failure errors)
    {
        Data = data;
        Errors = errors;
    }
}

public static class Envelope
{
    public static Envelope<T> Success<T>(T data) => new Envelope<T>(data, Failure.None);

    public static Envelope<T> Fail<T>(Failure errors) => new(default!,errors);
}
