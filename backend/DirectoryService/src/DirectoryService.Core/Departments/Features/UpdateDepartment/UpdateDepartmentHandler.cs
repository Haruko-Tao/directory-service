using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Core.Database;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.UpdateDepartment;

public sealed class UpdateDepartmentHandler : ICommandHandler<UpdateDepartmentCommand>
{                   
    private readonly IValidator<UpdateDepartmentCommand> _validator;
    private readonly ILogger<UpdateDepartmentHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ITransactionManager _transactionManager;
    
    public UpdateDepartmentHandler(IValidator<UpdateDepartmentCommand> validator,
    ILogger<UpdateDepartmentHandler> logger,
    IDepartmentsRepository departmentsRepository,
    ITransactionManager transactionManager)
    {
        _validator = validator;
        _logger = logger;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
    }
    
    public async Task<UnitResult<Failure>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));
        
        var departmentResult = await _departmentsRepository.GetByIdAsync(command.Id, cancellationToken);

        if (departmentResult.IsFailure)
        {
            _logger.LogWarning("Подразделение c {DepartmentId} не найдено", command.Id);
            return departmentResult.Error.ToFailure();
        }

        var nameResult = Name.Create(command.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        departmentResult.Value.Update(nameResult.Value);

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Подразделение {DepartmentId} успешно обновлено", command.Id);

        return UnitResult.Success<Failure>();
    }
}