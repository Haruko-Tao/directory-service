using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using HarukoTech.Shared.Kernel;
using FluentValidation;


namespace DirectoryService.Core.Departments.Features.SearchTree;

public class SearchTreeHandler : IQueryHandler<SearchTreeQuery, IReadOnlyCollection<DepartmentTreeNodeDto>>
{
    private readonly IDepartmentsReadRepository _readRepository;
    private readonly IValidator<SearchTreeQuery> _validator;

    public SearchTreeHandler(IDepartmentsReadRepository readRepository, IValidator<SearchTreeQuery> validator)
    {
        _readRepository = readRepository;
        _validator = validator;
    }

    public async Task<Result<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>> Handle(SearchTreeQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(e => (Error)e.CustomState!));
        
        var result = await _readRepository.GetSearchAsync(query.Search, cancellationToken);

        return Result.Success<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>(result);
    }
}