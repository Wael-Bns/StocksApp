namespace StocksApp.Core.Options
{
    public sealed class MatchBarBackupRetryOptions
    {
        public const string SectionName = "OrderMatching:BackupRetry";
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
        public int BatchSize { get; set; } = 100;
    }
}
