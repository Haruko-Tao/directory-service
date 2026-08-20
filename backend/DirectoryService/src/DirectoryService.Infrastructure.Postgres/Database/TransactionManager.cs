using CSharpFunctionalExtensions;
using DirectoryService.Core.Database;
using DirectoryService.Infrastructure.Postgres.Repositories;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres.Database;

public sealed class TransactionManager : ITransactionManager
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TransactionManager> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public TransactionManager(AppDbContext dbContext,
        ILogger<TransactionManager> logger,
        ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }
    
    public async Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            return new TransactionScope(transaction, _loggerFactory.CreateLogger<TransactionScope>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось начать транзакцию");
            return Error.Internal("transaction.begin.failed", "Ошибка при работе с БД");
        }
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Данные были изменены");
            return Error.Conflict("concurrency.conflict", "Данные были изменены другим пользователем");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            _logger.LogError(ex, "Ошибка у БД номер: {SqlState}", pg.SqlState);
            var error = pg.SqlState switch
            {
                "23505" => Error.Conflict("unique.violation", "Запись с таким значением уже существует"),
                "23503" => Error.Validation("reference.violation", "Ссылка на несуществующую запись"),
                _ => Error.Internal("database.failure", "Ошибка базы данных"),
            };

            return error;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить транзакцию");
            return Error.Internal("database.failure", "Ошибка базы данных");
        }
    }
}