using CSharpFunctionalExtensions;
using DirectoryService.Core.Database;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Database;

public sealed class TransactionScope : ITransactionScope
{
    private readonly IDbContextTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;
    
    public TransactionScope(IDbContextTransaction transaction,
        ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }
    
    public ValueTask DisposeAsync()
    {
        return _transaction.DisposeAsync();
    }

    public async Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _transaction.CommitAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось зафиксировать транзакцию");
            return Error.Internal("transaction.commit.failed", "Ошибка при сохранении данных в БД");
        }
        
    }

    public async Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _transaction.RollbackAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось откатить изменения");
            return Error.Internal("transaction.rollback.failed", "Ошибка при откате данных");
        }
    }
}