using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public class UpdateLocationsValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationsValidator()
    {
        RuleFor(u => u.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(u => u.Address)
            .MustBeValueObject(x => Address.Create(x.City, x.Street, x.House, x.Apartment));
    }
}