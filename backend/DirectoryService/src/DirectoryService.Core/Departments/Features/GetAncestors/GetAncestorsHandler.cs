using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using HarukoTech.Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.GetAncestors;

public class GetAncestorsHandler : IQueryHandler<GetAncestorsQuery, IReadOnlyCollection<DepartmentTreeNodeDto>>
{
    private readonly IDepartmentsReadRepository _readRepository;
    private readonly ILogger<GetAncestorsHandler> _logger;
    
    public GetAncestorsHandler(IDepartmentsReadRepository readRepository, ILogger<GetAncestorsHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }
    public async Task<Result<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>> Handle(GetAncestorsQuery query, CancellationToken cancellationToken)
    {
        var response = await _readRepository.ExistsAsync(query.Id, cancellationToken);

        if (!response)
        {
            _logger.LogWarning("Отдел с {DepartmentId} не найден", query.Id);
            return Error.NotFound("department.not.found", $"Отдел с {query.Id} не найден").ToFailure();
        }

        var result = await _readRepository.GetAncestorsAsync(query.Id, cancellationToken);

        return Result.Success<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>(result);
    }
}