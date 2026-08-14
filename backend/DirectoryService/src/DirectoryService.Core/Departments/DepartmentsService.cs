using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments.Extensions;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Path = DirectoryService.Domain.Departments.Path;
using Serilog.Context;

namespace DirectoryService.Core.Departments;

public class DepartmentsService
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<CreateDepartmentRequest> _validatorCreate;
    private readonly IValidator<UpdateDepartmentRequest> _validatorUpdate;
    private readonly IValidator<GetDepartmentsRequest> _validatorGetAll;
    private readonly ILogger<DepartmentsService> _logger;

    public DepartmentsService(ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreateDepartmentRequest> validatorCreate,
        IValidator<UpdateDepartmentRequest> validatorUpdate,
        IValidator<GetDepartmentsRequest> validatorGetAll, ILogger<DepartmentsService> logger)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _validatorCreate = validatorCreate;
        _validatorUpdate = validatorUpdate;
        _validatorGetAll = validatorGetAll;
        _logger = logger;
    }

    public async Task<Result<Guid, Failure>> Create(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validatorCreate.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));
        
        Path? parentPath = null;

        if (request.ParentId is not null)
        {
            var parent = await _departmentsRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);

            if (parent.IsFailure)
                return parent.Error.ToFailure();

            parentPath = parent.Value.Path;
        }

        foreach (var locationId in request.LocationIds)
        {
            var exist = await _locationsRepository.ExistsAsync(locationId, cancellationToken);
            if (!exist)
                return Error.NotFound("not.exist", "Локация не существует").ToFailure();
        }

        var nameResult = Name.Create(request.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();
        
        var slugResult = Slug.Create(request.Slug);

        if (slugResult.IsFailure)
            return slugResult.Error.ToFailure();
        
        var departmentResult =
            Department.Create(nameResult.Value, slugResult.Value, parentPath, request.ParentId);

        if (departmentResult.IsFailure)
            return departmentResult.Error.ToFailure();

        foreach (var locationId in request.LocationIds)
        {
            var departmentLocationResult = DepartmentLocation.Create(departmentResult.Value.Id, locationId: locationId);

            if (departmentLocationResult.IsFailure)
                return departmentLocationResult.Error.ToFailure();
            
            var addResultDepartmentLocationAsync = await _departmentsRepository.AddDepartmentLocationAsync(departmentLocationResult.Value, cancellationToken);
            if (addResultDepartmentLocationAsync.IsFailure)
                return addResultDepartmentLocationAsync.Error.ToFailure();
        }

        var addResult = await _departmentsRepository.AddAsync(departmentResult.Value, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToFailure();

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();

        return departmentResult.Value.Id;
    }

    public async Task<UnitResult<Failure>> Update(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validatorUpdate.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));
        
        var department = await _departmentsRepository.GetByIdAsync(id, cancellationToken);

        if (department.IsFailure)
        {
            _logger.LogWarning("Попытка найти подразделение c {DepartmentId} неуспешна", id);
            return department.Error.ToFailure();
        }

        var nameResult = Name.Create(request.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        department.Value.Update(nameResult.Value);

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Подразделение {DepartmentId} успешно обновлено", id);

        return UnitResult.Success<Failure>();
    }

    public async Task<UnitResult<Failure>> AddLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("DepartmentId", departmentId))
        using (LogContext.PushProperty("LocationId", locationId))
        {
            var existsLocation = await _locationsRepository.ExistsAsync(locationId, cancellationToken);
            var existsDepartment = await _departmentsRepository.ExistsAsync(departmentId, cancellationToken);

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
            var existsDepartmentLocationAsync = await _departmentsRepository.ExistsDepartmentLocationAsync(locationId, departmentId, cancellationToken);

            if (existsDepartmentLocationAsync)
            {
                _logger.LogWarning("Попытка повторной привязки уже существующей связи department-location");    
                return Error.Conflict("department.location.already_exists", "Связь отдела и локации уже существует").ToFailure();
            }
            var departmentLocation = DepartmentLocation.Create(departmentId, locationId);

            if (departmentLocation.IsFailure)
                return departmentLocation.Error.ToFailure();

            var addDepartmentLocationResult = await _departmentsRepository.AddDepartmentLocationAsync(departmentLocation.Value, cancellationToken);
            if (addDepartmentLocationResult.IsFailure)
                return addDepartmentLocationResult.Error.ToFailure();

            var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
            if (saveResult.IsFailure)
                return saveResult.Error.ToFailure();
            
            _logger.LogInformation("Локация успешно привязана к подразделению");

            return UnitResult.Success<Failure>();
        }
    }

    public async Task<UnitResult<Failure>> RemoveLocation(Guid locationId, Guid departmentId, CancellationToken cancellationToken)
    {
        var existsDepartmentLocation =
            await _departmentsRepository.ExistsDepartmentLocationAsync(locationId, departmentId, cancellationToken);

        if (!existsDepartmentLocation)
        {
            _logger.LogWarning("Неудачная попытка удаления связи {LocationId} и {DepartmentId}", locationId, departmentId);
            return Error.NotFound("department.location.not.found", "Связь отдела и локации не существует").ToFailure();
        }

        var removeResult = await _departmentsRepository.RemoveDepartmentLocationAsync(locationId, departmentId, cancellationToken);
        if (removeResult.IsFailure)
        {
            _logger.LogWarning("Попыка удаления локации {LocationId} от отдела {DepartmentId} была неуспешна", locationId, departmentId);
            return removeResult.Error.ToFailure();
        }

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();

        return UnitResult.Success<Failure>();
    }

    public async Task<Result<IReadOnlyList<DepartmentResponse>, Failure>> GetAll(GetDepartmentsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validatorGetAll.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));

        var addResult = await _departmentsRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

        return addResult.Select(l => l.ToResponse()).ToList();
    }
}