using DirectoryService.Contracts.Locations;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public class CreateLocationsValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Address.City)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.Address.Street)
            .NotEmpty()
            .MaximumLength(200);
        
        RuleFor(x => x.Address.House)
            .NotEmpty()
            .MaximumLength(200);
        
        
    }
}