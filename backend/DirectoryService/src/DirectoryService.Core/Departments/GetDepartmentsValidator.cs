using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Extensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Departments;

public class GetDepartmentsValidator : AbstractValidator<GetDepartmentsRequest>
{
    public GetDepartmentsValidator()
    {
        RuleFor(x => x.Page)
            .MustSatisfy(page =>
                page >= 1 
                    ? null 
                    : Error.Validation("page.invalid", "Страниц должно быть больше 1"));

        RuleFor(x => x.PageSize)
            .MustSatisfy(pagesize =>
                pagesize >= 1 && pagesize <= 100
                    ? null
                    : Error.Validation("pagesize.invalid", "Страниц должно быть меньше или равно 100 и больше 1"));
    }
}