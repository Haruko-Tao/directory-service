using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using HarukoTech.Shared.Kernel;
using FluentValidation;

namespace DirectoryService.Core.Locations.Features.UpdateLocation;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationsValidator()
    {
        RuleFor(u => u.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(u => u.AddressDto)
            .MustBeValueObject(x => x is null 
                ? Error.Validation("address.is.null", "Адрес не может быть пустым").ToFailure()
                : Address.Create(x.City, x.Street, x.House, x.Apartment));
    }
}