using System.Collections.Concurrent;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IntegrationTests;

// TODO(разобрать): написано ментором (DS-23).
// EF-интерцептор: через него проходит КАЖДАЯ SQL-команда, которую EF отправляет в БД.
// Считаем команды UPDATE по таблице departments.
// Две "двери": Reader (команды с результатом, ими часто ходит SaveChanges)
// и NonQuery (команды без результата, ими идёт ExecuteSqlInterpolatedAsync).
public sealed partial class UpdateCommandCounter : DbCommandInterceptor
{
    // \b — граница слова: ловит "UPDATE departments", но НЕ "updated_at" в SELECT-ах
    [GeneratedRegex(@"\bUPDATE\s+""?departments\b", RegexOptions.IgnoreCase)]
    private static partial Regex UpdateDepartmentsRegex();

    private int _count;

    public int Count => _count;

    public ConcurrentQueue<string> Commands { get; } = new();

    public void Reset()
    {
        Interlocked.Exchange(ref _count, 0);
        Commands.Clear();
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        CountIfUpdate(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CountIfUpdate(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void CountIfUpdate(DbCommand command)
    {
        // одна команда может содержать несколько UPDATE (батч SaveChanges) — считаем каждый
        var matches = UpdateDepartmentsRegex().Count(command.CommandText);
        if (matches == 0)
            return;

        Interlocked.Add(ref _count, matches);
        Commands.Enqueue(command.CommandText);
    }
}
