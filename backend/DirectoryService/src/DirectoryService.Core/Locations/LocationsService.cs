using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations.Exceptions;
using DirectoryService.Core.Locations.Extensions;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.SharedKernel;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace DirectoryService.Core.Locations;

public class LocationsService
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationRequest> _validatorCreate;
    private readonly IValidator<GetLocationsRequest> _validatorGetAll;
    private readonly IValidator<UpdateLocationRequest> _validatorUpdate;

    public LocationsService(ILocationsRepository locationsRepository,
        IValidator<CreateLocationRequest> validatorCreate,
        IValidator<GetLocationsRequest> validatorGetAll,
        IValidator<UpdateLocationRequest> validatorUpdate)
    {
        _locationsRepository = locationsRepository;
        _validatorCreate = validatorCreate;
        _validatorGetAll = validatorGetAll;
        _validatorUpdate = validatorUpdate;
    }

    public async Task<Result<Guid, Failure>> Create(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        //валидация входных данных
        var validationResult = await _validatorCreate.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return new Failure(validationResult.Errors.Select(v => (Error)v.CustomState!));
        }
        
        //валидация бизнес логики
        var isNameTaken = await _locationsRepository.IsNameTakenAsync(request.Name, cancellationToken);
        if (isNameTaken)
        {
            return  Error.Validation("is.name.taken", "Имя уже существует").ToFailure();
        }

        var nameResult = Name.Create(request.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();
        
        var addressResult = Address.Create(
            request.Address.City,
            request.Address.Street,
            request.Address.House,
            request.Address.Apartment);

        if (addressResult.IsFailure)
            return addressResult.Error;
        
        var locationResult = Location.Create(nameResult.Value, addressResult.Value);

        if (locationResult.IsFailure)
            return locationResult.Error.ToFailure();

        var addResult = await _locationsRepository.AddAsync(locationResult.Value, cancellationToken);

        if (addResult.IsFailure)
            return addResult.Error.ToFailure();

        return locationResult.Value.Id;
    }

    public async Task<UnitResult<Failure>> Update(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validatorUpdate.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));
        
        var location = await _locationsRepository.GetByIdAsync(id, cancellationToken);

        if (location.IsFailure)
            return location.Error.ToFailure();

        var nameResult = Name.Create(request.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        var addressResult = Address.Create(request.Address.City, request.Address.Street, request.Address.House, request.Address.Apartment);

        if (addressResult.IsFailure)
            return addressResult.Error;

        location.Value.Update(nameResult.Value, addressResult.Value);
        
        var saveResult = await _locationsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();

        return UnitResult.Success<Failure>();
    }

    public async Task<Result<IReadOnlyList<LocationResponse>, Failure>> GetAll(GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validatorGetAll.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));

        var addResult = await _locationsRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

        return addResult.Select(l => l.ToResponse()).ToList();
    }
}