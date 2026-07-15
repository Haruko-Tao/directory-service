using DirectoryService.Domain.Departments;

namespace DirectoryService.Domain.Locations;

public class Address
{
    public string Value { get; }

    private Address(string value)
    {
        Value = value;
    }

    public static Result<Address> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Address>.Failure("Адрес не может быть пустым!");

        if (value.Length > 50)
            return Result<Address>.Failure("Адрес не может быть длиннее 50 символов!");

        return Result<Address>.Success(new Address(value));
    }
}