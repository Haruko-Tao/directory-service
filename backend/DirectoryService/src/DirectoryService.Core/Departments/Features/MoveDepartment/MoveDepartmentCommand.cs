using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, Guid? ParentId) : ICommand;