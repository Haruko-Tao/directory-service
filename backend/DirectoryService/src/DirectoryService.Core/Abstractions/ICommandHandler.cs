using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Core.Abstractions;

#pragma warning disable CA1040
public interface ICommand {};
public interface IQuery {};
#pragma warning disable CA1040
public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery
{
    Task<Result<TResponse, Failure>> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand
{
    Task<Result<TResponse, Failure>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<TCommand> 
    where TCommand : ICommand
{
    Task<UnitResult<Failure>> Handle(TCommand command, CancellationToken cancellationToken);
}