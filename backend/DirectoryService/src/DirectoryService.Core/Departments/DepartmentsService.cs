using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments.Exceptions;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Locations.Exceptions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
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

    public async Task<Guid> Create(CreateDepartmentRequest request, CancellationToken cancellationToken)
    {
        Path? parentPath = null;

        if (request.ParentId is not null)
        {
            var parent = await _departmentsRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);

            if (parent == null)
                throw new DepartmentNotFoundException(request.ParentId.Value);

            parentPath = parent.Path;
        }

        foreach (var locationId in request.LocationIds)
        {
            var exist = await _locationsRepository.ExistsAsync(locationId, cancellationToken);
            if (!exist)
                throw new LocationNotFoundException(locationId);
        }

        var nameResult = Name.Create(request.Name);
        var slugResult = Slug.Create(request.Slug);

        var departmentResult =
            Department.Create(nameResult.Value!, slugResult.Value!, parentPath, request.ParentId);

        foreach (var locationId in request.LocationIds)
        {
            var departmentLocationResult = DepartmentLocation.Create(departmentResult.Value!.Id, locationId: locationId);
            await _departmentsRepository.AddDepartmentLocationAsync(departmentLocationResult.Value!, cancellationToken);
        }

        await _departmentsRepository.AddAsync(departmentResult.Value!, cancellationToken);

        await _departmentsRepository.SaveAsync(cancellationToken);

        return departmentResult.Value!.Id;
    }

    public async Task Update(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var department = await _departmentsRepository.GetByIdAsync(id, cancellationToken);
        
        if (department is null)
            throw new DepartmentNotFoundException(id);

        var nameResult = Name.Create(request.Name);

        department.Update(nameResult.Value!);

        await _departmentsRepository.SaveAsync(cancellationToken);
    }

    public async Task AddLocation(Guid departmentId, Guid locationId, CancellationToken cancellationToken)
    {
        var existsLocation = await _locationsRepository.ExistsAsync(locationId, cancellationToken);
        var existsDepartment = await _departmentsRepository.ExistsAsync(departmentId, cancellationToken);

        if (!existsDepartment || !existsLocation)
        {
            if (!existsDepartment)
                throw new DepartmentNotFoundException(departmentId);

            if (!existsLocation)
                throw new LocationNotFoundException(locationId);
        }

        var existsDepartmentLocationAsync = await _departmentsRepository.ExistsDepartmentLocationAsync(locationId, departmentId, cancellationToken);

        if (existsDepartmentLocationAsync)
            throw new DepartmentLocationAlReadyExistsException(departmentId, locationId);

        var departmentLocation = DepartmentLocation.Create(departmentId, locationId);

        await _departmentsRepository.AddDepartmentLocationAsync(departmentLocation.Value!, cancellationToken);

        await _departmentsRepository.SaveAsync(cancellationToken);
    }

    public async Task RemoveLocation(Guid locationId, Guid departmentId, CancellationToken cancellationToken)
    {
        var existsDepartmentLocation =
            await _departmentsRepository.ExistsDepartmentLocationAsync(locationId, departmentId, cancellationToken);

        if (!existsDepartmentLocation)
            throw new DepartmentLocationNotFoundException(departmentId, locationId);

        await _departmentsRepository.RemoveDepartmentLocationAsync(locationId, departmentId, cancellationToken);

        await _departmentsRepository.SaveAsync(cancellationToken);


    }
}