using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Departments;

public class Slug
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Slug>.Failure("Slug не может быть пустым");

        if (!Regex.IsMatch(value, "^[a-z0-9-]+$", RegexOptions.None, TimeSpan.FromSeconds(1)))
            return Result<Slug>.Failure("Slug может содержать только строчные латинские буквы, цифры и дефис");

        return Result<Slug>.Success(new Slug(value));
    }
}