using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace DirectoryService.Core.Locations;

public class LocationsService
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationRequest> _validator;

    public LocationsService(ILocationsRepository locationsRepository, IValidator<CreateLocationRequest> validator)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
    }

    public async Task<Guid> Create(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        //валидация входных данных
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        //валидация бизнес логики
        var isNameTaken = await _locationsRepository.IsNameTakenAsync(request.Name, cancellationToken);
        if (isNameTaken)
        {
            throw new Exception($"Локация с именем {request.Name} уже существует");
        }

        var nameResult = Name.Create(request.Name);
        
        var addressResult = Address.Create(
            request.Address.City,
            request.Address.Street,
            request.Address.House,
            request.Address.Apartment);
        
        var locationResult = Location.Create(nameResult.Value!, addressResult.Value!);

        await _locationsRepository.AddAsync(locationResult.Value!, cancellationToken);

        return locationResult.Value!.Id;
    }

    public async Task Update(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var location = await _locationsRepository.GetByIdAsync(id, cancellationToken);
        
        if (location is null)
            throw new Exception($"Локация с {id} не найдена");

        var nameResult = Name.Create(request.Name);

        var addressResult = Address.Create(request.Address.City, request.Address.Street, request.Address.House, request.Address.Apartment);

        var updateResult = location.Update(nameResult.Value!, addressResult.Value!);
        if (!updateResult.IsSuccess)
        {
            throw new Exception($"Не удалось обновить локацию с {id}");   
        }
        
        await _locationsRepository.SaveAsync(cancellationToken);
    }
}