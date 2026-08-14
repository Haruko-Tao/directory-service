using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Extensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Locations;

public class GetLocationsValidator : AbstractValidator<GetLocationsRequest>
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
                    : Error.Validation("pagesize.invalid", "Кол-во страниц должно быть больше 1 и меньше 100"));
    }
}