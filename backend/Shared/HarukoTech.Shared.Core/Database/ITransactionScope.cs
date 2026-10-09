using CSharpFunctionalExtensions;
using HarukoTech.Shared.Kernel;

namespace HarukoTech.Shared.Core.Database;

public interface ITransactionScope : IAsyncDisposable
{
    Task<UnitResult<Error>> CommitAsync(CancellationToken cancellationToken);

    Task<UnitResult<Error>> RollbackAsync(CancellationToken cancellationToken);
}