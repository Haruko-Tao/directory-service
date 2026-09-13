using Dapper;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Core.Locations;
using DirectoryService.Core.Locations.Features.GetLocations;
using Npgsql;


namespace DirectoryService.Infrastructure.Postgres.Repositories;

public sealed class DapperLocationsRepository : ILocationsReadRepository
{
    private readonly string _connectionString;

    private const string FilteredLocationsCte = """
                                             WITH filtered_locations AS (
                                             SELECT l.id, l.name, l.city, l.street, l.apartment, l.house, l.created_at, COUNT(d.id)::int AS department_count
                                             FROM locations l
                                             LEFT JOIN department_locations dl ON l.id = dl.location_id
                                             LEFT JOIN departments d ON d.id = dl.department_id AND d.is_deleted = FALSE
                                             WHERE(l.is_deleted = FALSE AND (@Search IS NULL OR UPPER(l.name) LIKE @Search))
                                             GROUP BY l.id
                                             HAVING (@MinDepartmentCount IS NULL OR @MinDepartmentCount <= COUNT(d.id)::int)
                                             )
                                             """;
    
    private sealed record LocationListRow(
        Guid Id,
        string Name,
        string City,
        string Street,
        string House,
        string? Apartment,
        DateTime CreatedAt,
        int DepartmentCount);
    
    public DapperLocationsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<PagedResult<LocationListItemDto>> GetPageAsync(GetLocationsQuery query,
        CancellationToken cancellationToken)
    {   
        var searchPattern = 
            string.IsNullOrWhiteSpace(query.Search) 
            ? null 
            : $"%{query.Search.ToUpperInvariant()}%";

        int offset = (query.Page - 1) * query.PageSize;

        string orderBy = (query.SortBy, query.SortDir) switch
        {
            ("NAME", "ASC") => "fl.name ASC",
            ("NAME", "DESC") => "fl.name DESC",
            ("CREATEDAT", "ASC") => "fl.created_at ASC",
            ("CREATEDAT", "DESC") => "fl.created_at DESC",
            ("DEPARTMENTCOUNT", "ASC") => "fl.department_count ASC",
            ("DEPARTMENTCOUNT", "DESC") => "fl.department_count DESC",
            _ => "fl.name ASC"
        };

        string sql = $"""
                      {FilteredLocationsCte}
                      SELECT COUNT(*)::int FROM filtered_locations;
                      
                      {FilteredLocationsCte}
                      SELECT fl.id, fl.name, fl.city, fl.street, fl.house, fl.apartment, fl.created_at AS CreatedAt, fl.department_count AS DepartmentCount
                      FROM filtered_locations fl
                      ORDER BY {orderBy}, fl.id
                      LIMIT @PageSize
                      OFFSET @Offset
                      """;

        var parameters = new
        {
            Search = searchPattern,
            MinDepartmentCount = query.MinDepartmentCount,
            Offset = offset,
            PageSize = query.PageSize
        };
        
        await using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await using var reader = await connection.QueryMultipleAsync(command);

        var totalCount = await reader.ReadSingleAsync<int>();

        var rows = await reader.ReadAsync<LocationListRow>();

        var items = rows.Select(x => new LocationListItemDto(
            x.Id,
            x.Name,
            new AddressDto(
                x.City,
                x.Street,
                x.House,
                x.Apartment),
            x.CreatedAt, x.DepartmentCount)).ToList();

        return new PagedResult<LocationListItemDto>(items, totalCount, query.Page, query.PageSize);
    }

    
}