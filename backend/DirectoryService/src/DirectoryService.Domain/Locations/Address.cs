using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel;

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

    public static Result<Address, Error> Create(string city, string street, string house, string? apartment)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Error.Validation("city.not.empty", "Город не может быть пустым");

        if (string.IsNullOrWhiteSpace(street))
            return Error.Validation("street.not.empty", "Улица не может быть пустой");

        if (string.IsNullOrWhiteSpace(house))
            return Error.Validation("house.not.empty", "Дом не может быть пустым");

        return new Address(city, street, house, apartment);
    }
}