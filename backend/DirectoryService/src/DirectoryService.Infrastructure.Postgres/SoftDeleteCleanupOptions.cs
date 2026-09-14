namespace DirectoryService.Infrastructure.Postgres;

public class SoftDeleteCleanupOptions
{
    public const string SectionName = "SoftDeleteCleanup";
    
    public TimeSpan Interval { get; set; }
    public TimeSpan RetentionPeriod { get; set; }
    public int BatchSize { get; set; }
}