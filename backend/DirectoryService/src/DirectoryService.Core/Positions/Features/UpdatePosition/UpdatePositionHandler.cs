using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Positions.Features.UpdatePosition;

public sealed class UpdatePositionHandler : ICommandHandler<UpdatePositionCommand>
{
    private readonly IPositionsRepository _repository;
    private readonly IValidator<UpdatePositionCommand> _validator;
    private readonly ILogger<UpdatePositionHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public UpdatePositionHandler(ILogger<UpdatePositionHandler> logger,
        IValidator<UpdatePositionCommand> validator,
        IPositionsRepository repository,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _validator = validator;
        _repository = repository;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Failure>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
    {
        var validateResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validateResult.IsValid)
            return new Failure(validateResult.Errors.Select(v => (Error)v.CustomState!));

        var position = await _repository.GetByIdAsync(command.Id, cancellationToken);

        if (position.IsFailure)
        {
            _logger.LogWarning("Позиция с {PositionId} не найдена", command.Id);
            return position.Error.ToFailure();
        }

        var nameResult = Name.Create(command.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        if (position.Value.Name != nameResult.Value)
        {
            var isNameTaken = await _repository.IsNameTakenAsync(nameResult.Value, cancellationToken);
            
            if (isNameTaken)
                return Error.Conflict("name.taken", $"Это имя {nameResult.Value.Value} уже занято").ToFailure();
        }

        position.Value.Update(nameResult.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Имя успешно обновлено c id {PositionId}", position.Value.Id);
        
        return UnitResult.Success<Failure>();
    }
}