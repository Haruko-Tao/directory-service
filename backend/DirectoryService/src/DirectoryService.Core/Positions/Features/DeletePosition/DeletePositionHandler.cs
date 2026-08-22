using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Departments;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Positions.Features.DeletePosition;

public sealed class DeletePositionHandler : ICommandHandler<DeletePositionCommand>
{
    private readonly IPositionsRepository _repository;
    private readonly ILogger<DeletePositionHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly IDepartmentsRepository _departmentsRepository;

    public DeletePositionHandler(ITransactionManager transactionManager,
        ILogger<DeletePositionHandler> logger,
        IPositionsRepository repository,
        IDepartmentsRepository departmentsRepository)
    {
        _transactionManager = transactionManager;
        _logger = logger;
        _repository = repository;
        _departmentsRepository = departmentsRepository;
    }

    public async Task<UnitResult<Failure>> Handle(DeletePositionCommand command, CancellationToken cancellationToken)
    {
        var positionResult = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (positionResult.IsFailure)
            return positionResult.Error.ToFailure();

        var countLinksPosition =
            await _departmentsRepository.CountLinksForPositionAsync(positionResult.Value.Id, cancellationToken);

        if (countLinksPosition > 0)
        {
            _logger.LogWarning("Должность {PositionId} используется в {CountPosition} подразделениях", positionResult.Value.Id, countLinksPosition);
            return Error.Conflict("position.in.use", $"Должность используется в {countLinksPosition} подразделениях")
                .ToFailure();
        }

        await _repository.RemoveAsync(positionResult.Value, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Позиция с {PositionId} удалена успешно", positionResult.Value.Id);
        
        return UnitResult.Success<Failure>();
    }
}