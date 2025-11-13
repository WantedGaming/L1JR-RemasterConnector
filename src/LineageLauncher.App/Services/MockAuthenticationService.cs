using System;
using System.Threading;
using System.Threading.Tasks;
using LineageLauncher.Core.Entities;
using LineageLauncher.Core.Interfaces;

namespace LineageLauncher.App.Services;

/// <summary>
/// Mock implementation of authentication service for UI testing.
/// Always returns successful authentication for any username/password.
/// </summary>
public class MockAuthenticationService : IAuthenticationService
{
    public async Task<User?> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Simulate network delay
        await Task.Delay(1000, cancellationToken);

        // Accept any non-empty username and password
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        // Return a mock authenticated user
        return User.Create(
            username: username,
            passwordHash: "mock_hash_" + password.GetHashCode(),
            rememberMe: false);
    }

    public async Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(500, cancellationToken);
        return !string.IsNullOrWhiteSpace(token);
    }

    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        // Mock sign out - nothing to do
        return Task.CompletedTask;
    }
}
