using DirectoryService.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.GetAncestors;

public record GetAncestorsQuery(Guid Id) : IQuery;