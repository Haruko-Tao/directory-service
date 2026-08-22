using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Positions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.AddPosition;

public sealed class AddPositionHandler : ICommandHandler<AddPositionCommand>
{
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<AddPositionHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IPositionsRepository _positionsRepository;

    public AddPositionHandler(IDepartmentsRepository departmentsRepository,
        ILogger<AddPositionHandler> logger,
        ITransactionManager transactionManager, IPositionsRepository positionsRepository)
    {
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _transactionManager = transactionManager;
        _positionsRepository = positionsRepository;
    }

    public async Task<UnitResult<Failure>> Handle(AddPositionCommand command, CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["DepartmentId"] = command.DepartmentId,
                   ["PositionId"] = command.PositionId
               }))
        {
            var existsPosition = await _positionsRepository.ExistsAsync(command.PositionId, cancellationToken);
            var existsDepartment = await _departmentsRepository.ExistsAsync(command.DepartmentId, cancellationToken);

            var errors = new List<Error>();
            
            if (!existsDepartment)
            {
                _logger.LogWarning("При привязке не найден отдел");
                errors.Add(Error.NotFound("department.not.found", $"Отдел с {command.DepartmentId} не найден"));
            }

            if (!existsPosition)
            {
                _logger.LogWarning("При привязке не найдена должность");
                errors.Add(Error.NotFound("position.not.found", $"Должность с {command.PositionId} не найдена"));
            }

            if (errors.Count > 0)
                return new Failure(errors);

            var existsDepartmentPosition =
                await _departmentsRepository.ExistsDepartmentPositionAsync(command.PositionId, command.DepartmentId,
                    cancellationToken);

            if (existsDepartmentPosition)
            {
                _logger.LogWarning("Связь отдела и должности уже существует");
                return Error.Conflict("department.position.already_exists",
                    $"Связь отдела с {command.DepartmentId} и должности {command.PositionId} уже существует").ToFailure();
            }

            var departmentPositionResult = DepartmentPosition.Create(command.DepartmentId, command.PositionId);

            if (departmentPositionResult.IsFailure)
                return departmentPositionResult.Error.ToFailure();

            await _departmentsRepository.AddDepartmentPositionAsync(departmentPositionResult.Value, cancellationToken);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (saveResult.IsFailure)
                return saveResult.Error.ToFailure();
            
            _logger.LogInformation("Связь между Должностью и Отделом успешно создана");
            
            return UnitResult.Success<Failure>();
        }
    }
}