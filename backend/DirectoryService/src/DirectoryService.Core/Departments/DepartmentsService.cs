using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments.Exceptions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Locations.Exceptions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.SharedKernel;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Core.Departments;

public class DepartmentsService
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;

    public DepartmentsService(ILocationsRepository locationsRepository, IDepartmentsRepository departmentsRepository)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
    }

    public async Task<Result<Guid, Failure>> Create(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
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
        var department = await _departmentsRepository.GetByIdAsync(id, cancellationToken);

        if (department.IsFailure)
            return department.Error.ToFailure();

        var nameResult = Name.Create(request.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        department.Value.Update(nameResult.Value);

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();

        return UnitResult.Success<Failure>();
    }

    public async Task<UnitResult<Failure>> AddLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken)
    {
        var existsLocation = await _locationsRepository.ExistsAsync(locationId, cancellationToken);
        var existsDepartment = await _departmentsRepository.ExistsAsync(departmentId, cancellationToken);

        if (!existsDepartment || !existsLocation)
        {
            if (!existsDepartment)
                return Error.NotFound("department.not.found", "Депаратамент не существует").ToFailure();

            if (!existsLocation)
                return Error.NotFound("location.not.found", "Локация не существует").ToFailure();
        }

        var existsDepartmentLocationAsync = await _departmentsRepository.ExistsDepartmentLocationAsync(locationId, departmentId, cancellationToken);

        if (existsDepartmentLocationAsync)
            return Error.Conflict("department.conflict", "Связь отдела и локации уже существует").ToFailure();

        var departmentLocation = DepartmentLocation.Create(departmentId, locationId);

        if (departmentLocation.IsFailure)
            return departmentLocation.Error.ToFailure();

        var addDepartmentLocationResult = await _departmentsRepository.AddDepartmentLocationAsync(departmentLocation.Value, cancellationToken);
        if (addDepartmentLocationResult.IsFailure)
            return addDepartmentLocationResult.Error.ToFailure();

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();

        return UnitResult.Success<Failure>();
    }

    public async Task<UnitResult<Failure>> RemoveLocation(Guid locationId, Guid departmentId, CancellationToken cancellationToken)
    {
        var existsDepartmentLocation =
            await _departmentsRepository.ExistsDepartmentLocationAsync(locationId, departmentId, cancellationToken);

        if (!existsDepartmentLocation)
            return Error.NotFound("department.location.not.found", "Связь отдела и локации не существует").ToFailure();

        var removeResult = await _departmentsRepository.RemoveDepartmentLocationAsync(locationId, departmentId, cancellationToken);
        if (removeResult.IsFailure)
            return removeResult.Error.ToFailure();

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();

        return UnitResult.Success<Failure>();
    }
}