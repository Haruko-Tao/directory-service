using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments;

public record Name
{
    public string Value { get; }

    private Name(string value)
    {
        Value = value;
    }

    public static Result<Name, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("name.not.space", "Имя не может быть пустым!");

        if (value.Length > 200)
            return Error.Validation("name.not.length", "Имя не может быть длинее 200 символов");

        return new Name(value);
    }
}