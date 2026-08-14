using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations;

public class Address
{
    public string City { get; }
    public string Street { get; }
    public string House { get; }
    public string? Apartment { get; }

    private Address(string city, string street, string house, string? apartment)
    {
        City = city;
        Street = street;
        House = house;
        Apartment = apartment;
    }

    public static Result<Address, Failure> Create(string city, string street, string house, string? apartment)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(city))
            errors.Add(Error.Validation("city.not.empty", "Город не может быть пустым"));

        if (string.IsNullOrWhiteSpace(street))
            errors.Add(Error.Validation("street.not.empty", "Улица не может быть пустой"));

        if (string.IsNullOrWhiteSpace(house))
            errors.Add(Error.Validation("house.not.empty", "Дом не может быть пустым"));

        if (errors.Count > 0)
            return new Failure(errors);

        return new Address(city, street, house, apartment);
    }
}