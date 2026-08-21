using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.RemoveLocation;

public sealed class RemoveLocationHandler : ICommandHandler<RemoveLocationCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILogger<RemoveLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;
    
    public RemoveLocationHandler(IDepartmentsRepository departmentsRepository,
        ILogger<RemoveLocationHandler> logger,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _transactionManager = transactionManager;
    }
    
    public async Task<UnitResult<Failure>> Handle(RemoveLocationCommand command, CancellationToken cancellationToken)
    {
        var removeResult = await _departmentsRepository.RemoveDepartmentLocationAsync(command.LocationId, command.DepartmentId, cancellationToken);
        if (removeResult.IsFailure)
        {
            _logger.LogWarning("Связь локации {LocationId} и отдела {DepartmentId} не найдена", command.LocationId, command.DepartmentId);
            return removeResult.Error.ToFailure();
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error.ToFailure();
        }
        
        _logger.LogInformation("Связь локации {LocationId} и отдела {DepartmentId} успешно удалена", command.LocationId, command.DepartmentId);

        return UnitResult.Success<Failure>();
    }
}