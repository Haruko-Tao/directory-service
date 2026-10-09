using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Database;
using DirectoryService.Domain.Departments;
using HarukoTech.Shared.Core.Abstractions;
using HarukoTech.Shared.Core.Database;
using HarukoTech.Shared.Kernel;

namespace DirectoryService.Core.Departments.Features.MoveDepartment;

public class MoveDepartmentHandler : ICommandHandler<MoveDepartmentCommand, MoveDepartmentResponse>
{
    private readonly IDepartmentsRepository _repository;
    private readonly ITransactionManager _transaction;
    
    public MoveDepartmentHandler(
        IDepartmentsRepository repository,
        ITransactionManager transaction)
    {
        _repository = repository;
        _transaction = transaction;
    }
    public async Task<Result<MoveDepartmentResponse, Failure>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken)
    {
        //Валидация
        if (command.DepartmentId == command.ParentId)
            return Error.Validation("department.move.parent_is_self", "Отдел не может быть перенесен внутрь себя").ToFailure();

        var transactionResult = await _transaction.BeginTransactionAsync(cancellationToken);

        if (transactionResult.IsFailure)
            return transactionResult.Error.ToFailure();

        //Открываем транзакцию для корректных ответов при параллельных запросах
        await using var transaction = transactionResult.Value;

        var lockResult = await _repository.LockForMoveAsync(command.DepartmentId, command.ParentId, cancellationToken);
        
        if (lockResult.IsFailure)
            return lockResult.Error.ToFailure();
        
        var departmentResult = await _repository.GetByIdAsync(command.DepartmentId, cancellationToken);

        if (departmentResult.IsFailure)
            return Error.NotFound("department.not_found", "Двигаемое подразделение не найдено").ToFailure();

        var department = departmentResult.Value;

        Department? parent = null;

        if (command.ParentId != null)
        {
            var parentResult = await _repository.GetByIdIncludingDeletedAsync(command.ParentId.Value, cancellationToken);

            if (parentResult.IsFailure)
                return Error.NotFound("department.parent.not_found", "Родитель не найден").ToFailure();

            if (parentResult.Value.IsDeleted)
                return Error.Conflict("department.move.parent_deleted", "Данный родитель удален").ToFailure();

            parent = parentResult.Value;
        }

        if (command.ParentId == department.ParentId)
        {
            return new MoveDepartmentResponse(
                    department.Id,
                    department.ParentId,
                    department.Path.Value,
                    department.Depth,
                    department.UpdatedAt);
        }

        if (parent?.Path.Value.StartsWith(department.Path.Value + ".", System.StringComparison.Ordinal) == true)
        {
           return Error.Conflict("department.move.cycle", "Нельзя перенести внутрь собственного поддерева").ToFailure();
        }

        department.ChangeParent(command.ParentId);

        var save = await _transaction.SaveChangesAsync(cancellationToken);

        if (save.IsFailure)
            return save.Error.ToFailure();

        var moveResult = await _repository.MoveSubtreeAsync(department.Path, parent?.Path, cancellationToken);

        if (moveResult.IsFailure)
            return moveResult.Error.ToFailure();

        var commitResult = await transaction.CommitAsync(cancellationToken);

        if (commitResult.IsFailure)
            return commitResult.Error.ToFailure();

        var reloadResult = await _repository.ReloadAsync(department, cancellationToken);

        if (reloadResult.IsFailure)
            return reloadResult.Error.ToFailure();

        return new MoveDepartmentResponse(department.Id, department.ParentId, department.Path.Value, department.Depth,
            department.UpdatedAt);
    }
}