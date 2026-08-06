using Dapper;
using DirectoryService.Core.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class DapperLocationsRepository : ILocationsRepository
{
    private readonly string _connectionString;
    private readonly ILogger<DapperLocationsRepository> _logger;

    public DapperLocationsRepository(IConfiguration configuration, ILogger<DapperLocationsRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }
    
    public async Task AddAsync(Location location, CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO locations (id, name, city, street, house, apartment, created_at, updated_at)
                           VALUES (@Id, @Name, @City, @Street, @House, @Apartment, @CreatedAt, @UpdatedAt)
                           """;

        var parameters = new
        {
            Id = location.Id,
            Name = location.Name.Value,
            City = location.Address.City,
            Street = location.Address.Street,
            House = location.Address.House,
            Apartment = location.Address.Apartment,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt
        };

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить локацию с id {LocationId}", location.Id);
            throw;
        }
        
    }

    public async Task<bool> IsNameTakenAsync(string name, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT EXISTS(
                           SELECT 1 FROM locations WHERE name = @Name
                           )
                           """;

        await using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition(sql, new { Name = name }, cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT EXISTS( 
                           SELECT 1 FROM locations WHERE id = @Id
                           )
                           """;

        await using var connection = new NpgsqlConnection(_connectionString);

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}