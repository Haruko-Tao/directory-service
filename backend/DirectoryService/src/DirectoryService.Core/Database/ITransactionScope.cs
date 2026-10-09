using CSharpFunctionalExtensions;
using HarukoTech.Shared.Kernel;

namespace DirectoryService.Core.Database;

public interface ITransactionScope : IAsyncDisposable
{
    Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken);

    Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken);
}