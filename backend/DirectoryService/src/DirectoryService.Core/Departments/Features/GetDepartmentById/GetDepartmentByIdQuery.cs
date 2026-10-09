using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.GetDepartmentById;

public sealed record GetDepartmentByIdQuery(Guid Id) : IQuery;