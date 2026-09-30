namespace StocksApp.Infrastructure.Options
{
    public sealed class LeaderElectionOptions
    {
        public const string SectionName = "LeaderElection";

        /// <summary>Must be a DIRECT Postgres connection, never a transaction-mode pooler.</summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>Must be positive.</summary>
        public long LockKey { get; set; } = 727_001;

        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(3);
        public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>How long the leader keeps leading while it cannot confirm the lock.</summary>
        public TimeSpan FenceAfter { get; set; } = TimeSpan.FromSeconds(45);
    }
}
