namespace StocksApp.Core.Options
{
    public class JwtOptions
    {
        public const string SectionName = "JWT";
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int EXPIRATION_MINUTES { get; set; }
    }
}
