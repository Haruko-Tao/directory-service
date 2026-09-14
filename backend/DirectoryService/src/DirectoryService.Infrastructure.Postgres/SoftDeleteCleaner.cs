using Dapper;
using DirectoryService.Core;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres;

public sealed class SoftDeleteCleaner : ISoftDeleteCleaner
{
    private readonly string _connectionString;
    
    private const string SelectExpiredLocationsSql = """
                                                     SELECT id FROM locations 
                                                     WHERE is_deleted AND deleted_at < @Threshold
                                                     ORDER BY deleted_at, id
                                                     LIMIT @BatchSize
                                                     """;

    private const string DeleteLocationLinkSql = """
                                                 DELETE FROM department_locations WHERE location_id = ANY(@Ids)
                                                 """;

    private const string DeleteLocationsSql = """
                                              DELETE FROM locations WHERE id = ANY(@Ids) 
                                              """;

    private const string SelectExpiredPositionSql = """
                                                    SELECT id FROM positions
                                                    WHERE is_deleted AND deleted_at < @Threshold
                                                    ORDER BY deleted_at, id
                                                    LIMIT @BatchSize
                                                    """;

    private const string DeletePositionLinksSql = """
                                                  DELETE FROM department_positions WHERE position_id = ANY(@Ids) 
                                                  """;

    private const string DeletePositionSql = """
                                             DELETE FROM positions WHERE id = ANY(@Ids) 
                                             """;

    private const string SelectExpiredDepartmentsSql = """
                                                       SELECT id FROM departments d 
                                                       WHERE d.is_deleted AND d.deleted_at < @Threshold AND NOT EXISTS(SELECT 1 FROM departments de WHERE de.parent_id = d.id)
                                                       ORDER BY d.deleted_at, d.id
                                                       LIMIT @BatchSize
                                                       """;

    private const string DeleteDepartmentLocationLinksSql = """
                                                            DELETE FROM department_locations WHERE department_id = ANY(@Ids) 
                                                            """;

    private const string DeleteDepartmentPositionLinksSql = """
                                                            DELETE FROM department_positions WHERE department_id = ANY(@Ids) 
                                                            """;

    private const string DeleteDepartmentsSql = """
                                                DELETE FROM departments WHERE id = ANY(@Ids) 
                                                """;

    public SoftDeleteCleaner(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<int> CleanupAsync(DateTime threshold, int batchSize, CancellationToken cancellationToken)
    {
        var deletedLocations = await PurgeLocationsAsync(threshold, batchSize, cancellationToken);
        var deletedPositions = await PurgePositionsAsync(threshold, batchSize, cancellationToken);
        var deletedDepartments = await PurgeDepartmentsAsync(threshold, batchSize, cancellationToken);

        return deletedLocations + deletedPositions + deletedDepartments;
    }

    private async Task<int> PurgeLocationsAsync(DateTime threshold, int batchSize, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        int total = 0;

        while (true)
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            var selectCommand = new CommandDefinition(SelectExpiredLocationsSql,
                new {Threshold = threshold, BatchSize = batchSize},
                transaction,
                cancellationToken: cancellationToken);

            var expiredLocationIds = (await connection.QueryAsync<Guid>(selectCommand)).ToArray();

            if (expiredLocationIds.Length == 0)
                break;

            var deleteLinksCommand = new CommandDefinition(DeleteLocationLinkSql,
                new {Ids = expiredLocationIds},
                transaction,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(deleteLinksCommand);

            var deleteLocationsCommand = new CommandDefinition(DeleteLocationsSql,
                new {Ids = expiredLocationIds},
                transaction,
                cancellationToken: cancellationToken);

            var deleteCount = await connection.ExecuteAsync(deleteLocationsCommand);

            await transaction.CommitAsync(cancellationToken);

            total += deleteCount;

            if (expiredLocationIds.Length < batchSize)
                break;
        }

        return total;
    }

    private async Task<int> PurgePositionsAsync(DateTime threshold, int batchSize, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        int total = 0;

        while (true)
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            var selectCommand = new CommandDefinition(SelectExpiredPositionSql,
                new { Threshold = threshold, BatchSize = batchSize}, transaction, cancellationToken: cancellationToken);

            var expiredPositionIds = (await connection.QueryAsync<Guid>(selectCommand)).ToArray();

            if (expiredPositionIds.Length == 0)
                break;

            var deleteLinksCommand = new CommandDefinition(DeletePositionLinksSql, new { Ids = expiredPositionIds },
                transaction, cancellationToken: cancellationToken);

            await connection.ExecuteAsync(deleteLinksCommand);

            var deletePositionCommand = new CommandDefinition(DeletePositionSql, new { Ids = expiredPositionIds },
                transaction, cancellationToken: cancellationToken);

            var deletePositionCount = await connection.ExecuteAsync(deletePositionCommand);

            await transaction.CommitAsync(cancellationToken);

            total += deletePositionCount;

            if (expiredPositionIds.Length < batchSize)
                break;
        }

        return total;
    }

    private async Task<int> PurgeDepartmentsAsync(DateTime threshold, int batchSize,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        int total = 0;

        while (true)
        {
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            var selectCommand = new CommandDefinition(SelectExpiredDepartmentsSql,
                new { Threshold = threshold, BatchSize = batchSize }, transaction,
                cancellationToken: cancellationToken);
            
            var expiredDepartments = (await connection.QueryAsync<Guid>(selectCommand)).ToArray();

            if (expiredDepartments.Length == 0)
                break;

            var deletePositionLinks = new CommandDefinition(DeleteDepartmentPositionLinksSql,
                new { Ids = expiredDepartments }, transaction, cancellationToken: cancellationToken);

            await connection.ExecuteAsync(deletePositionLinks);
            
            var deleteLocationLinks = new CommandDefinition(DeleteDepartmentLocationLinksSql,
                new { Ids = expiredDepartments }, transaction, cancellationToken: cancellationToken);

            await connection.ExecuteAsync(deleteLocationLinks);

            var deleteDepartmentCommand = new CommandDefinition(DeleteDepartmentsSql,
                new {Ids = expiredDepartments }, transaction,
                cancellationToken: cancellationToken);

            var deleteDepartmentCount = await connection.ExecuteAsync(deleteDepartmentCommand);

            await transaction.CommitAsync(cancellationToken);

            total += deleteDepartmentCount;
        }

        return total;
    }
}