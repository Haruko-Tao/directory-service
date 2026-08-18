using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.RemoveLocation;

public sealed class RemoveLocationHandler : ICommandHandler<RemoveLocationCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<RemoveLocationHandler> _logger;
    
    public RemoveLocationHandler(IDepartmentsRepository departmentsRepository,
        ILogger<RemoveLocationHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _logger = logger;
    }
    
    public async Task<UnitResult<Failure>> Handle(RemoveLocationCommand command, CancellationToken cancellationToken)
    {
        var existsDepartmentLocation =
            await _departmentsRepository.ExistsDepartmentLocationAsync(command.LocationId, command.DepartmentId, cancellationToken);

        if (!existsDepartmentLocation)
        {
            _logger.LogWarning("Неудачная попытка удаления связи {LocationId} и {DepartmentId}", command.LocationId, command.DepartmentId);
            return Error.NotFound("department.location.not.found", "Связь отдела и локации не существует").ToFailure();
        }

        var removeResult = await _departmentsRepository.RemoveDepartmentLocationAsync(command.LocationId, command.DepartmentId, cancellationToken);
        if (removeResult.IsFailure)
        {
            _logger.LogError("Попыка удаления локации {LocationId} от отдела {DepartmentId} была неуспешна", command.LocationId, command.DepartmentId);
            return removeResult.Error.ToFailure();
        }

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Связь локации {LocationId} и отдела {DepartmentId} успешно удалена", command.LocationId, command.DepartmentId);

        return UnitResult.Success<Failure>();
    }
}