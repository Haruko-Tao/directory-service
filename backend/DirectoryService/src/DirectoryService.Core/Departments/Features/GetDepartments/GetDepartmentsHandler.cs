using CSharpFunctionalExtensions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Departments.Extensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.GetDepartments;

public sealed class GetDepartmentsHandler : IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetDepartmentsQuery> _validator;
    
    public GetDepartmentsHandler(IReadDbContext readDbContext,
        IValidator<GetDepartmentsQuery> validator)
    {
        _readDbContext = readDbContext;
        _validator = validator;
    }
    
    public async Task<Result<PagedResult<DepartmentListItemDto>, Failure>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));

        var departmentsQuery = _readDbContext.Departments;
#pragma warning disable CA1304, CA1311 
        if (query.Search is not null)
        {
            var searchPattern = $"%{query.Search.ToUpperInvariant()}%";
            
            departmentsQuery =
                departmentsQuery.Where(d => 
                    EF.Functions.Like(d.Name.Value.ToUpper(), searchPattern));
        }
#pragma warning restore CA1304, CA1311

        IOrderedQueryable<Department> sortedQuery = (query.SortBy, query.SortDir) switch
        {
            ("NAME", "ASC") => departmentsQuery.OrderBy(d => d.Name.Value),
            ("NAME", "DESC") => departmentsQuery.OrderByDescending(d => d.Name.Value),
            ("CREATEDAT", "ASC") => departmentsQuery.OrderBy(d => d.CreatedAt),
            _ => departmentsQuery.OrderByDescending(d => d.CreatedAt),
        };

        sortedQuery = sortedQuery.ThenBy(d => d.Id);

        int totalCount = await departmentsQuery.CountAsync(cancellationToken);

        var items = await sortedQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new DepartmentListItemDto(
                    d.Id,
                    d.Name.Value,
                    d.Slug.Value,
                    d.Path.Value,
                    d.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<DepartmentListItemDto>(items, totalCount, query.Page, query.PageSize);

        return result;

    }
}