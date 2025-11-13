using LineageLauncher.Core.Entities;

namespace LineageLauncher.Core.Interfaces;

/// <summary>
/// Defines the contract for user authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with the L1JR-Server.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The plain-text password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authenticated user or null if authentication fails.</returns>
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a stored authentication token.
    /// </summary>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
