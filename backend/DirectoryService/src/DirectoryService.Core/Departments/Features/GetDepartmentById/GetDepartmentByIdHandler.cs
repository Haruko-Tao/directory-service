using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.GetDepartmentById;

public sealed class GetDepartmentByIdHandler : IQueryHandler<GetDepartmentByIdQuery, DepartmentResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetDepartmentByIdHandler> _logger;

    public GetDepartmentByIdHandler(IReadDbContext readDbContext,
        ILogger<GetDepartmentByIdHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }
    public async Task<Result<DepartmentResponse, Failure>> Handle(GetDepartmentByIdQuery query, CancellationToken cancellationToken)
    {
        var departmentResponse = await _readDbContext.Departments
            .Where(d => d.Id == query.Id)
            .Select(d =>
            new DepartmentResponse(d.Id, d.Name.Value, d.Slug.Value, d.Path.Value, d.ParentId, d.CreatedAt, d.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (departmentResponse is null)
        {
            _logger.LogWarning("Отдел с {DepartmentId} не найден", query.Id);
            return Error.NotFound("department.not.found", $"Отдел с {query.Id} не найден").ToFailure();
        }

        return departmentResponse;
    }
}