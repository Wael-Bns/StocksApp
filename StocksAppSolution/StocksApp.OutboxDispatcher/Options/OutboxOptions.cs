namespace StocksApp.OutboxDispatcher.Options
{
    public class OutboxOptions
    {
        public const string SectionName = "Outbox";
        public string ConnectionString { get; set; } = string.Empty;
        public string NotificationChannel { get; set; } = "outbox_inserted";
        public TimeSpan FallbackPollInterval { get; set; } = TimeSpan.FromSeconds(10);
        public int BatchSize { get; set; } = 100;
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);
    }
}
