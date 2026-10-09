using HarukoTech.Shared.Core.Abstractions;

namespace DirectoryService.Core.Departments.Features.SearchTree;

public record SearchTreeQuery(string Search) : IQuery;