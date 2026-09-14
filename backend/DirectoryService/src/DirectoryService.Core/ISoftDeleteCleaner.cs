namespace DirectoryService.Core;

public interface ISoftDeleteCleaner
{
    Task<int> CleanupAsync(DateTime threshold, int batchSize, CancellationToken cancellationToken);
}