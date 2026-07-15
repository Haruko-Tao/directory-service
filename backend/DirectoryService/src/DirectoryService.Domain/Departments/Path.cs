namespace DirectoryService.Domain.Departments;

public class Path
{
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Result<Path> Create(string slug, Path? parentPath)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return Result<Path>.Failure("Slug не может бытть пустым при построении пути");

        var value = parentPath is null ? slug : $"{parentPath.Value}/{slug}";

        return Result<Path>.Success(new Path(value));
    }
}