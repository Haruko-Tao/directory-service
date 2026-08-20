using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Locations.Features.UpdateLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Locations.Features.UpdateLocation;

public sealed class UpdateLocationHandler : ICommandHandler<UpdateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<UpdateLocationCommand> _validator;
    private readonly ILogger<UpdateLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    
    public UpdateLocationHandler(ILocationsRepository locationsRepository,
        IValidator<UpdateLocationCommand> validator,
        ILogger<UpdateLocationHandler> logger,
        ITransactionManager transactionManager)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
        _transactionManager = transactionManager;
    }
    
    public async Task<UnitResult<Failure>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));
        
        var locationResult = await _locationsRepository.GetByIdAsync(command.LocationId, cancellationToken);

        if (locationResult.IsFailure)
        {
            _logger.LogWarning("Локация {LocationId} не найдена", command.LocationId);
            return locationResult.Error.ToFailure();
        }

        var nameResult = Name.Create(command.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        var addressResult = Address.Create(command.AddressDto.City, command.AddressDto.Street, command.AddressDto.House, command.AddressDto.Apartment);

        if (addressResult.IsFailure)
            return addressResult.Error;

        locationResult.Value.Update(nameResult.Value, addressResult.Value);
        
        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Локация с {LocationId} успешно обновлена", command.LocationId);

        return UnitResult.Success<Failure>();
    }
}