using Dapper;
using DirectoryService.Contracts.Departments;
using DirectoryService.Core.Departments;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class DapperDepartmentsRepository : IDepartmentsReadRepository
{
    private readonly string _connectionString;
    
    public DapperDepartmentsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetRootsAsync(CancellationToken cancellationToken)
    {
        string sql = """
                     SELECT d.id, d.name, d.slug, d.path::text, d.depth,
                            EXISTS(
                         SELECT 1
                         FROM departments child
                         WHERE child.parent_id = d.id
                           AND child.is_deleted = false
                     ) AS hasChildren
                     FROM departments d
                     WHERE d.is_deleted = false
                       AND d.depth = 0
                     ORDER BY d.created_at, d.id
                     """;
        
        await using var connection = new NpgsqlConnection(_connectionString);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<DepartmentTreeNodeDto>(command)).AsList();

        return rows;
    }

    public async Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetChildrenAsync(Guid id, CancellationToken cancellationToken)
    {
        string sql = """
                     SELECT id, name, slug, path, depth, 
                            EXISTS(
                                SELECT 1
                                FROM departments child
                                WHERE child.parent_id = d.id
                                 AND child.is_deleted = false
                            ) AS hasChildren
                     FROM departments d
                     WHERE d.parent_id = @Id
                         AND d.is_deleted = false
                     ORDER BY d.created_at, d.id;
                     """;

        var parameters = new { Id = id };

        await using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return (await connection.QueryAsync<DepartmentTreeNodeDto>(command)).AsList();
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        string sql = """
                     SELECT EXISTS(
                     SELECT 1
                     FROM departments d 
                     WHERE d.id = @Id
                     AND d.is_deleted = false);
                     """;

        var parameters = new { Id = id };

        await using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return await connection.QueryFirstAsync<bool>(command);

    }

    public async Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetAncestorsAsync(Guid id, CancellationToken cancellationToken)
    {
        string sql = """
                     SELECT a.id, a.name, a.slug, a.path, a.depth, true AS hasChildren
                     FROM departments n
                     JOIN departments a ON a.path @> n.path        
                     WHERE n.id = @Id 
                         AND a.is_deleted = false AND a.path <> n.path
                     ORDER BY a.depth, a.created_at 
                     """;

        var parameters = new { Id = id };

        await using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return (await connection.QueryAsync<DepartmentTreeNodeDto>(command)).AsList();
    }

    public async Task<IReadOnlyCollection<DepartmentTreeNodeDto>> GetSearchAsync(string query, CancellationToken cancellationToken)
    {
        string sql = """
                     SELECT d.id, d.name, d.slug, d.path, d.depth,
                            EXISTS(
                                SELECT 1 
                                FROM departments child
                                WHERE child.parent_id = d.id
                                     AND child.is_deleted = false
                            ) AS hasChildren FROM departments d
                     WHERE d.name ILIKE @Search 
                         AND d.is_deleted = false
                     ORDER BY depth, created_at
                     LIMIT 50 
                     """;

        var searchPatter = $"%{query}%";

        var parameters = new { Search = searchPatter };

        await using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        return (await connection.QueryAsync<DepartmentTreeNodeDto>(command)).AsList();
    }
}