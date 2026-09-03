using DirectoryService.Core.Extensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Departments.Features.GetDepartments;

public sealed class GetDepartmentsValidator : AbstractValidator<GetDepartmentsQuery>
{
    public GetDepartmentsValidator()
    {
        RuleFor(x => x.Page)
            .MustSatisfy(page =>
                page >= 1 
                    ? null 
                    : Error.Validation("page.invalid", "Страниц должно быть больше 0"));

        RuleFor(x => x.PageSize)
            .MustSatisfy(pagesize =>
                pagesize >= 1 && pagesize <= 100
                    ? null
                    : Error.Validation("page.size.invalid", "Размер страницы должен быть не больше 100 и больше 1"));

        RuleFor(x => x.Search)
            .MustSatisfy(s =>
                string.IsNullOrEmpty(s) || s.Length <= 100
                    ? null
                    : Error.Validation("search.invalid", "Длина имени поиска должна быть не большее 100"));

        RuleFor(x => x.SortBy)
            .MustSatisfy(s =>
                s == "CREATEDAT" || s == "NAME"
                    ? null 
                    : Error.Validation("sort.by.invalid", "Неправильное имя для сортировки"));

        RuleFor(x => x.SortDir)
            .MustSatisfy(s =>
                s == "DESC" || s == "ASC"
                    ? null
                    : Error.Validation("sort.dir.invalid", "Некорректное имя для сортировки"));

    }
}