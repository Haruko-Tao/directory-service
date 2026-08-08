using DirectoryService.Contracts.Locations;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public class CreateLocationsValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationsValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя не может быть пустым")
            .MaximumLength(200).WithMessage("Максимальная длина 200");

        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("Город должен быть указан")
            .MaximumLength(100).WithMessage("Максимальная длина 100");
        
        RuleFor(x => x.Address.Street)
            .NotEmpty().WithMessage("Улица должна быть указана")
            .MaximumLength(200).WithMessage("Максимальная длина 200");

        RuleFor(x => x.Address.House)
            .NotEmpty().WithMessage("Дом должен быть указан")
            .MaximumLength(200).WithMessage("Максимальная длина 200");


    }
}