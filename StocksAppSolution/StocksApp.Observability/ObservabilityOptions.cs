namespace StocksApp.Observability
{
    public class ObservabilityOptions
    {
        public const string SectionName = "Observability";
        public bool Enabled { get; set; } = true;
        public string ServiceName { get; set; } = default!;
        public string? OtlpEndpoint { get; set; }
    }
}
