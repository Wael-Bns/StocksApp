namespace StocksApp.Observability
{
    public class ObservabilityOptions
    {
        public const string SectionName = "Observability";
        public string ServiceName { get; set; } = default!;
        public string OtlpEndpoint { get; set; } = "http://alloy:4317";
    }
}
