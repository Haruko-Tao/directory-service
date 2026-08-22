using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Positions.Features.UpdatePosition;

public sealed class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);
    }
}