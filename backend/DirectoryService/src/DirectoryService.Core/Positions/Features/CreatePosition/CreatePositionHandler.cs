using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Positions.Features.CreatePosition;

public sealed class CreatePositionHandler : ICommandHandler<CreatePositionCommand, Guid>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public CreatePositionHandler(ILogger<CreatePositionHandler> logger,
        IValidator<CreatePositionCommand> validator,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager)
    {
        _logger = logger;
        _validator = validator;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
    }
    public async Task<Result<Guid, Failure>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(v => (Error)v.CustomState!));

        var nameResult = Name.Create(command.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        var isNameTakenAsync = await _positionsRepository.IsNameTakenAsync(nameResult.Value, cancellationToken);

        if (isNameTakenAsync)
        {
            _logger.LogWarning("Позиция уже существует {PositionName}", command.Name);
            return Error.Conflict("name.position.taken", $"Должность с именем {command.Name} уже существует").ToFailure();
        }

        var positionResult = Position.Create(nameResult.Value);

        if (positionResult.IsFailure)
            return positionResult.Error.ToFailure();
        
        await _positionsRepository.AddAsync(positionResult.Value, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Позиция успешно создана с id {Id}", positionResult.Value.Id);

        return positionResult.Value.Id;
    }
    


}