using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Locations.Features.CreateLocation;

public sealed class CreateLocationHandler : ICommandHandler<CreateLocationCommand, Guid>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    
    public CreateLocationHandler(ILocationsRepository locationsRepository,
        ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationCommand> validator,
        ITransactionManager transactionManager)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
        _transactionManager = transactionManager;
    }
    
    public async Task<Result<Guid, Failure>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        //валидация входных данных
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return new Failure(validationResult.Errors.Select(v => (Error)v.CustomState!));
        }
        
        //валидация бизнес логики
        var nameResult = Name.Create(command.Name);
        
        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();
        
        var isNameTaken = await _locationsRepository.IsNameTakenAsync(nameResult.Value, cancellationToken);
        if (isNameTaken)
        {
            _logger.LogWarning("Имя локации {LocationName} уже существует", command.Name);
            return  Error.Conflict("is.name.taken", "Имя уже существует").ToFailure();
        }
        
        var addressResult = Address.Create(
            command.AddressDto.City,
            command.AddressDto.Street,
            command.AddressDto.House,
            command.AddressDto.Apartment);

        if (addressResult.IsFailure)
            return addressResult.Error;
        
        var locationResult = Location.Create(nameResult.Value, addressResult.Value);

        if (locationResult.IsFailure)
            return locationResult.Error.ToFailure();

        await _locationsRepository.AddAsync(locationResult.Value, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();

        _logger.LogInformation("Локация {LocationId} создана", locationResult.Value.Id);
        
        return locationResult.Value.Id;
    }
}