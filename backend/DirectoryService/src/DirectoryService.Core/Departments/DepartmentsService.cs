using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Locations;
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
                throw new Exception("Родитель не найден");

            parentPath = parent.Path;
        }

        foreach (var locationId in request.LocationIds)
        {
            var exist = await _locationsRepository.ExistsAsync(locationId, cancellationToken);
            if (!exist)
                throw new Exception($"Локация {locationId} для отдела не найдены");
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
}