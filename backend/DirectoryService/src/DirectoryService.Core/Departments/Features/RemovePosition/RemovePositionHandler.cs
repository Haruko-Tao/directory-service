using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.RemovePosition;

public sealed class RemovePositionHandler : ICommandHandler<RemovePositionCommand>
{
    private readonly ILogger<RemovePositionHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IDepartmentsRepository _departmentsRepository;

    public RemovePositionHandler(IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILogger<RemovePositionHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Failure>> Handle(RemovePositionCommand command, CancellationToken cancellationToken)
    {
        var removeResult = await _departmentsRepository.RemoveDepartmentPositionAsync(command.PositionId, command.DepartmentId,
                cancellationToken);

        if (removeResult.IsFailure)
        {
            _logger.LogWarning("Связи не существует между Отделом с Id {DepartmentId} и Должностью {PositionId}", command.DepartmentId, command.PositionId);
            return removeResult.Error.ToFailure();
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Связь отдела с {DepartmentId} и должности {PositionId} удалена", command.DepartmentId, command.PositionId);

        return UnitResult.Success<Failure>();
    }
}