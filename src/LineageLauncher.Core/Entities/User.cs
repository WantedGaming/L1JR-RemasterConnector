namespace LineageLauncher.Core.Entities;

/// <summary>
/// Represents a user account in the Lineage Remastered system.
/// </summary>
public sealed class User
{
    public required string Username { get; init; }
    public string? PasswordHash { get; init; }
    public bool RememberMe { get; init; }
    public DateTime? LastLoginAt { get; init; }

    /// <summary>
    /// Authentication token received from server after successful login.
    /// </summary>
    public string? AuthToken { get; init; }

    /// <summary>
    /// Indicates whether the user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated { get; init; }

    public static User Create(string username, string passwordHash, bool rememberMe = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            Username = username,
            PasswordHash = passwordHash,
            RememberMe = rememberMe,
            LastLoginAt = null,
            AuthToken = null,
            IsAuthenticated = false
        };
    }
}
