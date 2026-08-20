using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments.Features.UpdateDepartments;

public class UpdateDepartmentsValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentsValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);
    }
}