using CSharpFunctionalExtensions;
using HarukoTech.Shared.Kernel;

namespace HarukoTech.Shared.Core.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}