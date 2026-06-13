namespace StocksApp.Core.ServiceContracts
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes the provided password using a secure hashing algorithm and returns the resulting hash.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password.</returns>
        string HashPassword(string password);
        /// <summary>
        /// Verifies if the provided password matches the hashed password.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="hash">The hashed password to compare against.</param>
        /// <returns>True if the password matches the hash, otherwise false.</returns>
        bool VerifyPassword(string password, string hash);
    }
}