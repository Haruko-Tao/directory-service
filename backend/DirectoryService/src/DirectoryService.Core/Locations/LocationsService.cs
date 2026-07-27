using DirectoryService.Contracts.Locations;
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
        var addressResult = Address.Create(request.Address.Value);
        var locationResult = Location.Create(nameResult.Value!, addressResult.Value!);

        await _locationsRepository.AddAsync(locationResult.Value!, cancellationToken);

        return locationResult.Value!.Id;
    }
}