using System;
using System.Threading;
using System.Threading.Tasks;
using LineageLauncher.Core.Entities;
using LineageLauncher.Core.Interfaces;

namespace LineageLauncher.App.Services;

/// <summary>
/// Mock implementation of launcher service for UI testing.
/// Simulates game launching without actually starting Lin.bin.
/// </summary>
public class MockLauncherService : ILauncherService
{
    private bool _isGameRunning;
    private int _mockProcessId;

    public async Task<int> LaunchGameAsync(
        ServerInfo server,
        User user,
        CancellationToken cancellationToken = default)
    {
        // Simulate game launch delay
        await Task.Delay(2000, cancellationToken);

        // Generate a mock process ID
        _mockProcessId = Random.Shared.Next(1000, 9999);
        _isGameRunning = true;

        // In a real implementation, this would:
        // 1. Verify Lin.bin exists
        // 2. Prepare command-line arguments with server info
        // 3. Start the process
        // 4. Return the actual process ID

        return _mockProcessId;
    }

    public bool IsGameRunning()
    {
        return _isGameRunning;
    }

    public Task TerminateGameAsync(CancellationToken cancellationToken = default)
    {
        _isGameRunning = false;
        _mockProcessId = 0;
        return Task.CompletedTask;
    }
}
