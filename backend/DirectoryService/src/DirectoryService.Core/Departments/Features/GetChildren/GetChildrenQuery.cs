using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.GetChildren;

public record GetChildrenQuery(Guid Id) : IQuery;