using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.AddLocation;

public sealed class AddLocationHandler : ICommandHandler<AddLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<AddLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public AddLocationHandler(ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        ILogger<AddLocationHandler> logger,
        ITransactionManager transactionManager)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Failure>> Handle(AddLocationCommand command, CancellationToken cancellationToken)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["DepartmentId"] = command.DepartmentId,
                   ["LocationId"] = command.LocationId
               }))
        {
            var existsLocation = await _locationsRepository.ExistsAsync(command.LocationId, cancellationToken);
            var existsDepartment = await _departmentsRepository.ExistsAsync(command.DepartmentId, cancellationToken);

            var errors = new List<Error>();

            if (!existsDepartment)
            {
                _logger.LogWarning("Отдел не найден при привязке к Локации");
                errors.Add(Error.NotFound("department.not.found", "Отдел не существует"));
            }

            if (!existsLocation)
            {
                _logger.LogWarning("Локация не найдена при привязке к отделу");
                errors.Add(Error.NotFound("location.not.found", "Локация не существует"));
            }

            if (errors.Count > 0)
            {
                return new Failure(errors);
            }

            var existsLink =
                await _departmentsRepository.ExistsDepartmentLocationAsync(command.LocationId, command.DepartmentId,
                    cancellationToken);

            if (existsLink)
            {
                _logger.LogWarning("Попытка повторной привязки уже существующей связи department-location");
                return Error.Conflict("department.location.already_exists", "Связь отдела и локации уже существует")
                    .ToFailure();
            }

            var departmentLocation = DepartmentLocation.Create(command.DepartmentId, command.LocationId);

            if (departmentLocation.IsFailure)
                return departmentLocation.Error.ToFailure();
            
            await _departmentsRepository.AddDepartmentLocationAsync(departmentLocation.Value, cancellationToken);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            
            if (saveResult.IsFailure)
                return saveResult.Error.ToFailure();

            _logger.LogInformation("Локация успешно привязана к подразделению");

            return UnitResult.Success<Failure>();
        }
    }
}