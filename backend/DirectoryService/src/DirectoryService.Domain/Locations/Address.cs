using DirectoryService.Domain.Departments;

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

    public static Result<Address> Create(string city, string street, string house, string? apartment)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Result<Address>.Failure("Город не может быть пустым");
        
        if (string.IsNullOrWhiteSpace(street))
            return Result<Address>.Failure("Улица не может быть пустой");

        if (string.IsNullOrWhiteSpace(house))
            return Result<Address>.Failure("Дом не может быть пустым");

        return Result<Address>.Success(new Address(city, street, house, apartment));
    }
}