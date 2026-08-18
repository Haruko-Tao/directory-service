using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Departments.Extensions;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.GetDepartments;

public class GetDepartmentHandler : IQueryHandler<GetDepartmentsQuery, IReadOnlyCollection<DepartmentResponse>>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<GetDepartmentsQuery> _validator;
    
    public GetDepartmentHandler(IDepartmentsRepository departmentsRepository,
        IValidator<GetDepartmentsQuery> validator)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
    }
    
    public async Task<Result<IReadOnlyCollection<DepartmentResponse>, Failure>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));

        var addResult = await _departmentsRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

        return addResult.Select(l => l.ToResponse()).ToList();
    }
}