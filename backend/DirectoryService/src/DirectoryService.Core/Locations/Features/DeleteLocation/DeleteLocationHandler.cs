using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Core.Departments;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Locations.Features.DeleteLocation;

public sealed class DeleteLocationHandler : ICommandHandler<DeleteLocationCommand>
{
    private readonly ILogger<DeleteLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;

    public DeleteLocationHandler(ILocationsRepository locationsRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteLocationHandler> logger,
        IDepartmentsRepository departmentsRepository)
    {
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _departmentsRepository = departmentsRepository;
    }

    public async Task<UnitResult<Failure>> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var locationResult =  await _locationsRepository.GetByIdAsync(command.Id, cancellationToken);

        if (locationResult.IsFailure)
        {
            _logger.LogWarning("Локация {LocationId} не найдена", command.Id);
            return locationResult.Error.ToFailure();
        }

        var countDepartmentLocation =
            await _departmentsRepository.CountLinksForLocationAsync(command.Id, cancellationToken);

        if (countDepartmentLocation > 0)
        {
            _logger.LogWarning("Нельзя удалить локации {LocationId} пока есть связь с отделами - {CountDepartment}", command.Id, countDepartmentLocation);
            return Error.Conflict("location.in.use", $"Локация используется в {countDepartmentLocation} отделах").ToFailure();
        }

        await _locationsRepository.RemoveAsync(locationResult.Value, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Локация с {LocationId} успешно удалена", command.Id);
        
        return UnitResult.Success<Failure>();
    }
}