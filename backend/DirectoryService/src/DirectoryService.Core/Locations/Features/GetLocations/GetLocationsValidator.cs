using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Extensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Locations.Features.GetLocations;

public class GetLocationsValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationsValidator()
    {
        RuleFor(x => x.Page)
            .MustSatisfy(
                page => page >= 1 
                    ? null 
                    : Error.Validation("page.invalid", "Страниц должно быть не меньше 1"));

        RuleFor(x => x.PageSize)
            .MustSatisfy(pagesize =>
                pagesize <= 100 && pagesize >= 1
                    ? null
                    : Error.Validation("page.size.invalid", "Размер страницы должен быть не больше 100 и не меньше 1"));

        RuleFor(x => x.Search)
            .MustSatisfy(search =>
                string.IsNullOrWhiteSpace(search) || search.Length <= 100
                    ? null
                    : Error.Validation("search.invalid", "Запрос не может быть длиннее 100 символов"));

        RuleFor(x => x.SortBy)
            .MustSatisfy(sortBy =>
                sortBy == "CREATEDAT" || sortBy == "NAME" || sortBy == "DEPARTMENTCOUNT"
                    ? null
                    : Error.Validation("sort.by.invalid", "Запрос можно не сортировать или сортировать по имени, дате создания"));

        RuleFor(x => x.SortDir)
            .MustSatisfy(sortDir =>
                sortDir == "DESC" || sortDir == "ASC"
                    ? null
                    : Error.Validation("sort.dir.invalid", "Запрос для сортировки либо пустой либо по desc/asc"));

        RuleFor(x => x.MinDepartmentCount)
            .MustSatisfy(minDepartmentCount =>
                minDepartmentCount is null || minDepartmentCount >= 0
                    ? null
                    : Error.Validation("min.department.count.invalid", "Количество отделов не может быть отрицательным"));
    }
}