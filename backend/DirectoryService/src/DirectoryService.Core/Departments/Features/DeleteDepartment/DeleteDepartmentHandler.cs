using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.DeleteDepartment;

public sealed class DeleteDepartmentHandler : ICommandHandler<DeleteDepartmentCommand>
{
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<DeleteDepartmentHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;

    public DeleteDepartmentHandler(IDepartmentsRepository departmentsRepository,
        ILogger<DeleteDepartmentHandler> logger,
        ITransactionManager transactionManager)
    {
        _departmentsRepository = departmentsRepository;
        _logger = logger;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Failure>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
    {
        var departmentResult = await _departmentsRepository.GetByIdAsync(command.Id, cancellationToken);

        if (departmentResult.IsFailure)
        {
            _logger.LogWarning("Отдел с {DepartmentId} не найден", command.Id);
            return departmentResult.Error.ToFailure();
        }

        var countLinksForPosition =
            await _departmentsRepository.CountPositionLinksForDepartmentAsync(command.Id, cancellationToken);
        
        var countLinksForLocation =
            await _departmentsRepository.CountLocationLinksForDepartmentAsync(command.Id, cancellationToken);

        var countChildrenAsync = await _departmentsRepository.CountChildrenAsync(command.Id, cancellationToken);

        if (countChildrenAsync > 0 || countLinksForLocation > 0 || countLinksForPosition > 0)
        {
            _logger.LogWarning(
                "Отдел {DepartmentId} нельзя удалить: локаций - {LocationLinks}, должностей - {PositionLinks}, дочерних зависимостей - {ChildrenLinks}",
                command.Id, countLinksForLocation, countLinksForPosition, countChildrenAsync);

            return Error.Conflict("department.in.use",
                    $"Отдел используется: локация - {countLinksForLocation}, должностей - {countLinksForPosition}, дочерних подразделений - {countChildrenAsync}")
                .ToFailure();
        }
        
        await _departmentsRepository.RemoveAsync(departmentResult.Value, cancellationToken);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Отдел c {DepartmentId} успешно удалён", command.Id);
        
        return UnitResult.Success<Failure>();
        
    }
}