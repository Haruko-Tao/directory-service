using DirectoryService.Core.Extensions;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Core.Departments.Features.SearchTree;

public class SearchTreeValidator : AbstractValidator<SearchTreeQuery>
{
    public SearchTreeValidator()
    {
        RuleFor(d => d.Search)
            .MustSatisfy(s => 
                !string.IsNullOrEmpty(s) && s.Length >= 2
                ? null
                : Error.Validation("search.tree.invalid", "Поисковый запрос должен быть минимум 2 символа"));
    }
}