using LineageLauncher.Core.Entities;

namespace LineageLauncher.Core.Interfaces;

/// <summary>
/// Defines the contract for launching the Lineage game client.
/// </summary>
public interface ILauncherService
{
    /// <summary>
    /// Launches the Lin.bin game client with the specified parameters.
    /// </summary>
    /// <param name="server">Server information for connection.</param>
    /// <param name="user">Authenticated user information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The process ID of the launched game client.</returns>
    Task<int> LaunchGameAsync(ServerInfo server, User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the game client is currently running.
    /// </summary>
    bool IsGameRunning();

    /// <summary>
    /// Terminates the running game client process.
    /// </summary>
    Task TerminateGameAsync(CancellationToken cancellationToken = default);
}
