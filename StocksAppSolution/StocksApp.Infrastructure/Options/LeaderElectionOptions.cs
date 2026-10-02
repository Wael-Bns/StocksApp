namespace StocksApp.Infrastructure.Options
{
    public sealed class LeaderElectionOptions
    {
        public const string SectionName = "LeaderElection";

        /// <summary>Must be a DIRECT Postgres connection, never a transaction-mode pooler.</summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>Must be positive. Every instance competing for the same leadership must use the same key.</summary>
        public long LockKey { get; set; } = 727_001;

        /// <summary>
        /// The only setting most deployments need: how long a leader keeps leading while it
        /// cannot reach Postgres, before it steps down. Everything below is derived from it.
        /// </summary>
        public TimeSpan FenceAfter { get; set; } = TimeSpan.FromSeconds(45);

        // ---- Advanced overrides. Leave at 00:00:00 to derive them from FenceAfter. ----

        /// <summary>How often the leader checks its lock. Derived: FenceAfter / 15.</summary>
        public TimeSpan HeartbeatInterval { get; set; }

        /// <summary>Max duration of one leadership check. Derived: FenceAfter / 15.</summary>
        public TimeSpan HeartbeatTimeout { get; set; }

        /// <summary>How often a standby tries to take the lock. Derived: FenceAfter / 9.</summary>
        public TimeSpan RetryInterval { get; set; }

        /// <summary>Fills every timing left at zero. Runs before validation.</summary>
        internal void ApplyDerivedTimings()
        {
            if (FenceAfter <= TimeSpan.Zero) return;   // the validator reports it

            if (HeartbeatInterval == TimeSpan.Zero) HeartbeatInterval = FenceAfter / 15;
            if (HeartbeatTimeout == TimeSpan.Zero) HeartbeatTimeout = FenceAfter / 15;
            if (RetryInterval == TimeSpan.Zero) RetryInterval = FenceAfter / 9;
        }
    }
}