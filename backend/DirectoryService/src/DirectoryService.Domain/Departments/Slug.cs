using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.SharedKernel;

namespace DirectoryService.Domain.Departments;

public class Slug
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Result<Slug, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("slug.not.space", "Slug не может быть пустым!");

        if (!Regex.IsMatch(value, "^[a-z0-9-]+$", RegexOptions.None, TimeSpan.FromSeconds(1)))
            return Error.Validation("slug.regex",
                "Slug может содержать только строчные латинские буквы, цифры и дефис");

        return new Slug(value);
    }
}