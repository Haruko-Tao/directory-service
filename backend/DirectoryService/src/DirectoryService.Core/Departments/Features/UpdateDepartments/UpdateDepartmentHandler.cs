using CSharpFunctionalExtensions;
using DirectoryService.Core.Abstractions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Core.Departments.Features.UpdateDepartments;

public class UpdateDepartmentHandler : ICommandHandler<UpdateDepartmentCommand>
{                   
    private readonly IValidator<UpdateDepartmentCommand> _validator;
    private readonly ILogger<UpdateDepartmentHandler> _logger;
    private readonly IDepartmentsRepository _departmentsRepository;
    
    public UpdateDepartmentHandler(IValidator<UpdateDepartmentCommand> validator,
    ILogger<UpdateDepartmentHandler> logger,
    IDepartmentsRepository departmentsRepository)
    {
        _validator = validator;
        _logger = logger;
        _departmentsRepository = departmentsRepository;
    }
    
    public async Task<UnitResult<Failure>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return new Failure(validationResult.Errors.Select(l => (Error)l.CustomState!));
        
        var department = await _departmentsRepository.GetByIdAsync(command.Id, cancellationToken);

        if (department.IsFailure)
        {
            _logger.LogWarning("Попытка найти подразделение c {DepartmentId} неуспешна", command.Id);
            return department.Error.ToFailure();
        }

        var nameResult = Name.Create(command.Name);

        if (nameResult.IsFailure)
            return nameResult.Error.ToFailure();

        department.Value.Update(nameResult.Value);

        var saveResult = await _departmentsRepository.SaveAsync(cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToFailure();
        
        _logger.LogInformation("Подразделение {DepartmentId} успешно обновлено", command.Id);

        return UnitResult.Success<Failure>();
    }
}