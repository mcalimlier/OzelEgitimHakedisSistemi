namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Writes a group of archive snapshots as one logical operation. If any write
/// fails, files created by this operation are removed in reverse order so the
/// UI cannot silently leave a partial multi-route archive batch.
/// </summary>
public static class ProgressPaymentArchiveBatchService
{
    public sealed record Result(
        bool Success,
        IReadOnlyList<string> WrittenPaths,
        IReadOnlyList<string> RollbackFailures,
        string? Error);

    public static Result Execute<T>(
        IEnumerable<T> items,
        Func<T, (bool Success, string? Path, string? Error)> write,
        Action<string> rollback)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(rollback);

        var materialized = items.ToArray();
        if (materialized.Length == 0)
            return new Result(false, Array.Empty<string>(), Array.Empty<string>(),
                "Arşivlenecek hakediş kaydı bulunamadı.");

        var writtenPaths = new List<string>(materialized.Length);
        foreach (var item in materialized)
        {
            (bool Success, string? Path, string? Error) result;
            try
            {
                result = write(item);
            }
            catch (Exception ex) when (
                ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
            {
                return Rollback(writtenPaths, rollback, ex.Message);
            }

            if (!result.Success || string.IsNullOrWhiteSpace(result.Path))
            {
                return Rollback(
                    writtenPaths,
                    rollback,
                    string.IsNullOrWhiteSpace(result.Error)
                        ? "Arşiv yazımı başarısız oldu."
                        : result.Error);
            }

            writtenPaths.Add(result.Path);
        }

        return new Result(true, writtenPaths.ToArray(), Array.Empty<string>(), null);
    }

    private static Result Rollback(
        IReadOnlyList<string> writtenPaths,
        Action<string> rollback,
        string error)
    {
        var rollbackFailures = new List<string>();
        for (var index = writtenPaths.Count - 1; index >= 0; index--)
        {
            var path = writtenPaths[index];
            try
            {
                rollback(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                rollbackFailures.Add($"{Path.GetFileName(path)}: {ex.Message}");
            }
        }

        return new Result(false, Array.Empty<string>(), rollbackFailures, error);
    }
}
