using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Positions.Features.CreatePosition;

public sealed class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);
    }
}