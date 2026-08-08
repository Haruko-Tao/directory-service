using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Fails;
using DirectoryService.Core.Locations.Exceptions;
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
            throw new ValidationFailException(validationResult.Errors.Select(v => v.ErrorMessage));
        }
        
        //валидация бизнес логики
        var isNameTaken = await _locationsRepository.IsNameTakenAsync(request.Name, cancellationToken);
        if (isNameTaken)
        {
            throw new LocationNameDuplicateException(request.Name);
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
            throw new LocationNotFoundException(id);

        var nameResult = Name.Create(request.Name);

        var addressResult = Address.Create(request.Address.City, request.Address.Street, request.Address.House, request.Address.Apartment);

        location.Update(nameResult.Value!, addressResult.Value!);
        
        await _locationsRepository.SaveAsync(cancellationToken);
    }
}