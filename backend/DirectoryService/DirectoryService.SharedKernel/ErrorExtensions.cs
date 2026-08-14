namespace DirectoryService.Shared;

public static class ErrorExtensions
{
    public static Failure ToFailure(this Error error) => error;
}