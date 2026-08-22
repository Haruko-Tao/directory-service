using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Locations.Features.CreateLocation;

public sealed class CreateLocationsValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationsValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.AddressDto)
            .MustBeValueObject(a => a is null
                ? Error.Validation("address.is.null", " Адрес должен быть указан").ToFailure()
                : Address.Create(a.City, a.Street, a.House, a.Apartment));
    }
}