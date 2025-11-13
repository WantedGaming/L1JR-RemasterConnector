namespace LineageLauncher.Core.Interfaces;

/// <summary>
/// Defines the contract for password hashing operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using Argon2id.
    /// </summary>
    /// <param name="password">The plain-text password.</param>
    /// <returns>The hashed password string.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that a plain-text password matches a hashed password.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hashedPassword">The hashed password to compare against.</param>
    /// <returns>True if the password matches; otherwise, false.</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
