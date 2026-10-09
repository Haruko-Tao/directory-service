using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Abstractions;
using HarukoTech.Shared.Kernel;

namespace DirectoryService.Core.Departments.Features.GetDepartmentsTree;

public sealed class GetDepartmentsTreeHandler : IQueryHandler<GetDepartmentsTreeQuery, IReadOnlyCollection<DepartmentTreeNodeDto>>
{
    private readonly IDepartmentsReadRepository _readRepository;

    public GetDepartmentsTreeHandler(IDepartmentsReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>> Handle(GetDepartmentsTreeQuery query, CancellationToken cancellationToken)
    {
        var result = await _readRepository.GetRootsAsync(cancellationToken);

        return Result.Success<IReadOnlyCollection<DepartmentTreeNodeDto>, Failure>(result);
    }
}