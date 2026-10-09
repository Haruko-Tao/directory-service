using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using HarukoTech.Shared.Core.Abstractions;
using HarukoTech.Shared.Kernel;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.GetChildren;

public class GetChildrenHandler : IQueryHandler<GetChildrenQuery,IReadOnlyCollection<DepartmentTreeNodeDto>>
{
    private readonly IDepartmentsReadRepository _readRepository;
    private readonly ILogger<GetChildrenHandler> _logger;
    
    public GetChildrenHandler(IDepartmentsReadRepository readRepository, ILogger<GetChildrenHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }
    public async Task<Result<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>> Handle(GetChildrenQuery query, CancellationToken cancellationToken)
    {
        var response = await _readRepository.ExistsAsync(query.Id, cancellationToken);

        if (!response)
        {
            _logger.LogWarning("Отдел с {DepartmentId} не найден", query.Id);
            return Error.NotFound("department.not.found", $"Отдел с {query.Id} не найден").ToFailure();
        }

        var result = await _readRepository.GetChildrenAsync(query.Id, cancellationToken);
        
        return Result.Success<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>(result);
    }
}