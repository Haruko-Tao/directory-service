using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
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

    public AddLocationHandler(ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        ILogger<AddLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _logger = logger;
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
                errors.Add(Error.NotFound("department.not.found", "Депаратамент не существует"));

            if (!existsLocation)
                errors.Add(Error.NotFound("location.not.found", "Локация не существует"));

            if (errors.Count > 0)
            {
                _logger.LogWarning("Не удалось привязать локацию: department и location не найдены");
                return new Failure(errors);
            }

            var existsDepartmentLocationAsync =
                await _departmentsRepository.ExistsDepartmentLocationAsync(command.LocationId, command.DepartmentId,
                    cancellationToken);

            if (existsDepartmentLocationAsync)
            {
                _logger.LogWarning("Попытка повторной привязки уже существующей связи department-location");
                return Error.Conflict("department.location.already_exists", "Связь отдела и локации уже существует")
                    .ToFailure();
            }

            var departmentLocation = DepartmentLocation.Create(command.DepartmentId, command.LocationId);

            if (departmentLocation.IsFailure)
                return departmentLocation.Error.ToFailure();

            var addDepartmentLocationResult =
                await _departmentsRepository.AddDepartmentLocationAsync(departmentLocation.Value, cancellationToken);
            if (addDepartmentLocationResult.IsFailure)
                return addDepartmentLocationResult.Error.ToFailure();

            var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
            if (saveResult.IsFailure)
                return saveResult.Error.ToFailure();

            _logger.LogInformation("Локация успешно привязана к подразделению");

            return UnitResult.Success<Failure>();
        }
    }
}