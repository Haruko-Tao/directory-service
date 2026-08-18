using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Core.Departments.Features.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<CreateDepartmentCommand, Guid>
{
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    
    public CreateDepartmentHandler(IValidator<CreateDepartmentCommand> validator,
    ILogger<CreateDepartmentHandler> logger,
    IDepartmentsRepository departmentsRepository,
    ILocationsRepository locationsRepository)
    {
        _validator = validator;
        _logger = logger;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
    }
    
    public async Task<Result<Guid, Failure>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));
        
        Path? parentPath = null;

        if (command.ParentId is not null)
        {
            var parent = await _departmentsRepository.GetByIdAsync(command.ParentId.Value, cancellationToken);

            if (parent.IsFailure)
                return parent.Error.ToFailure();

            parentPath = parent.Value.Path;
        }

        foreach (var locationId in command.LocationIds)
        {
            var exist = await _locationsRepository.ExistsAsync(locationId, cancellationToken);
            if (!exist)
            {
                _logger.LogWarning("Локация с {LocationId} не существует", locationId);
                return Error.NotFound("not.exist", "Локация не существует").ToFailure();
            }
        }

        var nameResult = Name.Create(command.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();
        
        var slugResult = Slug.Create(command.Slug);

        if (slugResult.IsFailure)
        {
            return slugResult.Error.ToFailure();
        }

        var isSlugTaken = await _departmentsRepository.IsSlugTakenAsync(slugResult.Value.Value, command.ParentId, cancellationToken);

        if (isSlugTaken)
        {
            _logger.LogWarning("Slug который вы выбрали - {ParentId}/{Slug} уже занят", command.ParentId,command.Slug);
            return Error.Conflict("is.slug.taken", $"Такой {command.Slug} уже занят").ToFailure();
        }
        
        var departmentResult =
            Department.Create(nameResult.Value, slugResult.Value, parentPath, command.ParentId);

        if (departmentResult.IsFailure)
            return departmentResult.Error.ToFailure();

        foreach (var locationId in command.LocationIds)
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
}