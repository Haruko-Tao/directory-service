namespace DirectoryService.Domain.Departments;

public class Name
{
    public string Value { get; }

    private Name(string value)
    {
        Value = value;
    }

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Name>.Failure("Название не может быть пустым");

        if (value.Length > 200)
            return Result<Name>.Failure("Название не может быть длиннее 200 символов");

        return Result<Name>.Success(new Name(value));
    }
}