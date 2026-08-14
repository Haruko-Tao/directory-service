using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Extensions;
using DirectoryService.Domain.Departments;
using FluentValidation;

namespace DirectoryService.Core.Departments;

public class UpdateDepartmentsValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentsValidator()
    {
        RuleFor(x => x.Name)
            .MustBeValueObject(Name.Create);
    }
}