namespace DirectoryService.Domain.Departments;

public class Department
{
    public Guid Id { get; private set; }
    
    public Name Name { get; private set; }
    
    public Slug Slug { get; private set; }
    
    public Path Path { get; private set; }
    
    public Guid? ParentId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }
    
    //EF Core
    private Department()
    {
        Name = null!;
        Slug = null!;
        Path = null!;
    }

    private Department(Guid id, Name name, Slug slug, Path path, Guid? parentId, DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        Name = name;
        Slug = slug;
        Path = path;
        ParentId = parentId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Department> Create(Name name, Slug slug, Path? parentPath, Guid? parentId)
    {
        var pathResult = Path.Create(slug.Value, parentPath);

        if (!pathResult.IsSuccess)
            return Result<Department>.Failure(pathResult.Error!);

        var department = new Department(
            Guid.NewGuid(), name, slug, pathResult.Value!, parentId, DateTime.UtcNow, DateTime.UtcNow);

        return Result<Department>.Success(department);
    }
}