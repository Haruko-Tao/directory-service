namespace DirectoryService.Contracts.Departments;

public record CreateDepartmentRequest(string Name, string Slug, Guid? ParentId);