namespace StocksApp.Core.Options
{
    public class RefreshTokenOptions
    {
        public const string SectionName = "RefreshToken";
        public int EXPIRATION_MINUTES { get; set; }
    }
}
