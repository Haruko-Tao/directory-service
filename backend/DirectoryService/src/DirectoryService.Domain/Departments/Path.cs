using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments;

public class Path
{
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Result<Path, Error> Create(string slug, Path? parentPath)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return Error.Validation("slug.not.space", "Slug не может быть пустым при построении пути!");

        var value = parentPath is null ? slug : $"{parentPath.Value}/{slug}";

        return new Path(value);
    }
}