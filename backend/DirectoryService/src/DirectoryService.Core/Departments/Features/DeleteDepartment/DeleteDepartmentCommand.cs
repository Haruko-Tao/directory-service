using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;
