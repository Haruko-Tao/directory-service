namespace DirectoryService.Contracts.Departments;

public record MoveDepartmentResponse(Guid Id, Guid? ParentId, string Path, int Depth, DateTime UpdatedAt);