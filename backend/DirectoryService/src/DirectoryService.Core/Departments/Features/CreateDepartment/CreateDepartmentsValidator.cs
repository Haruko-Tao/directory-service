using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments.Features.CreateDepartment;

public class CreateDepartmentsValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentsValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}