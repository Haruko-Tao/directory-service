namespace HarukoTech.Shared.Kernel;

public class DomainException : Exception
{
    public Error Error { get; }
    
    public DomainException(Error error) : base(string.Join("; ", error.Message))
    {
        Error = error;
    }
}