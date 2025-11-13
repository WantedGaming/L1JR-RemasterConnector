using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineageLauncher.Launcher.Process;
using LineageLauncher.Launcher.Injection;
using LineageLauncher.Launcher.IPC;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher.Orchestration;

/// <summary>
/// Orchestrates the complete process launch with DLL injection and IPC setup.
/// </summary>
public sealed class ProcessLaunchOrchestrator : IDisposable
{
    private readonly ILogger<ProcessLaunchOrchestrator> _logger;
    private readonly ProcessCreator _processCreator;
    private readonly DllInjector _dllInjector;
    private readonly PipeManager _pipeManager;
    private bool _disposed;

    public bool IsProcessRunning => _processCreator.IsRunning;
    public int ProcessId => _processCreator.ProcessId;

    public ProcessLaunchOrchestrator(
        ILogger<ProcessLaunchOrchestrator> logger,
        ProcessCreator processCreator,
        DllInjector dllInjector,
        PipeManager pipeManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processCreator = processCreator ?? throw new ArgumentNullException(nameof(processCreator));
        _dllInjector = dllInjector ?? throw new ArgumentNullException(nameof(dllInjector));
        _pipeManager = pipeManager ?? throw new ArgumentNullException(nameof(pipeManager));
    }

    /// <summary>
    /// Launches the game with DLL injection following the complete flow.
    /// </summary>
    public async Task<LaunchResult> LaunchWithInjectionAsync(
        LaunchConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Starting Launch with Injection ===");
        _logger.LogInformation("Executable: {Path}", config.ExecutablePath);
        _logger.LogInformation("DLLs to inject: {Count}", config.DllsToInject.Count);
        _logger.LogInformation("Enable Pipes: {EnablePipes}", config.EnablePipes);

        var currentPhase = LaunchPhase.None;

        try
        {
            // Phase 1: Create Named Pipes (Optional - can trigger anti-cheat detection)
            if (config.EnablePipes)
            {
                currentPhase = LaunchPhase.PipeCreation;
                progress?.Report("Creating IPC pipes...");
                _logger.LogInformation("Phase 1: Creating named pipes");

                await _pipeManager.CreatePipesAsync(cancellationToken);

                _logger.LogInformation("✓ Phase 1 complete");
            }
            else
            {
                _logger.LogInformation("Phase 1: Skipped (pipes disabled to avoid anti-cheat detection)");
            }

            // Phase 2: Create Process (Suspended)
            currentPhase = LaunchPhase.ProcessCreation;
            progress?.Report("Creating game process...");
            _logger.LogInformation("Phase 2: Creating suspended process");

            var createResult = _processCreator.CreateSuspended(
                config.ExecutablePath,
                config.CommandLine ?? string.Empty,
                config.WorkingDirectory,
                config.EnvironmentVariables);

            if (!createResult.Success)
            {
                _logger.LogError(
                    "Process creation failed: {Error}",
                    createResult.ErrorMessage);

                return LaunchResult.CreateFailed(
                    currentPhase,
                    createResult.ErrorMessage);
            }

            _logger.LogInformation(
                "✓ Phase 2 complete (PID: {ProcessId})",
                createResult.ProcessId);

            // Phase 3: Inject DLLs
            currentPhase = LaunchPhase.DllInjection;
            progress?.Report($"Injecting {config.DllsToInject.Count} DLLs...");
            _logger.LogInformation("Phase 3: Injecting DLLs");

            var injectionResult = await _dllInjector.InjectDllsAsync(
                createResult.ProcessHandle,
                config.DllsToInject,
                cancellationToken);

            if (!injectionResult.Success)
            {
                _logger.LogError(
                    "DLL injection failed: {Error}",
                    injectionResult.ErrorMessage);

                // Rollback: Terminate process
                _logger.LogWarning("Rolling back: Terminating process");
                _processCreator.Terminate();

                return LaunchResult.CreateFailed(
                    currentPhase,
                    $"DLL injection failed: {injectionResult.ErrorMessage}");
            }

            _logger.LogInformation(
                "✓ Phase 3 complete ({Count} DLLs injected)",
                injectionResult.InjectedCount);

            // Phase 4: Resume Main Thread
            currentPhase = LaunchPhase.ThreadResume;
            progress?.Report("Starting game...");
            _logger.LogInformation("Phase 4: Resuming main thread");

            _processCreator.ResumeMainThread();

            _logger.LogInformation("✓ Phase 4 complete");

            // Phase 5: Wait for Pipe Connection (Optional - only if pipes enabled)
            if (config.EnablePipes)
            {
                currentPhase = LaunchPhase.PipeConnection;
                progress?.Report("Waiting for game initialization...");
                _logger.LogInformation("Phase 5: Waiting for pipe connection");

                try
                {
                    await _pipeManager.WaitForConnectionAsync(
                        config.PipeConnectionTimeout,
                        cancellationToken);

                    _logger.LogInformation("✓ Phase 5 complete (Pipe connected)");
                }
                catch (TimeoutException)
                {
                    _logger.LogWarning(
                        "Pipe connection timeout (this may be normal behavior)");
                }
            }
            else
            {
                _logger.LogInformation("Phase 5: Skipped (pipes disabled)");
            }

            // Success
            progress?.Report("Game launched successfully!");
            _logger.LogInformation("=== Launch Complete ===");

            return LaunchResult.CreateSuccess(createResult.ProcessId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Launch cancelled by user");
            await TerminateAsync();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during launch");
            await TerminateAsync();

            return LaunchResult.CreateFailed(
                currentPhase,
                $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Terminates the running game process.
    /// </summary>
    public Task TerminateAsync()
    {
        _logger.LogInformation("Terminating game process...");
        _processCreator.Terminate();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _processCreator.Dispose();
        _dllInjector.Dispose();
        _pipeManager.Dispose();

        _disposed = true;
        _logger.LogDebug("ProcessLaunchOrchestrator disposed");
    }
}

/// <summary>
/// Configuration for launching a process with DLL injection.
/// </summary>
public sealed class LaunchConfiguration
{
    public required string ExecutablePath { get; init; }
    public required string WorkingDirectory { get; init; }
    public required List<string> DllsToInject { get; init; }
    public string? CommandLine { get; init; }
    public Dictionary<string, string>? EnvironmentVariables { get; init; }
    public bool EnablePipes { get; init; } = false; // Disabled by default to avoid anti-cheat detection
    public TimeSpan PipeConnectionTimeout { get; init; } = TimeSpan.FromSeconds(10);
}

/// <summary>
/// Result of the complete launch operation.
/// </summary>
public sealed class LaunchResult
{
    public bool Success { get; init; }
    public int ProcessId { get; init; }
    public string? ErrorMessage { get; init; }
    public LaunchPhase FailedPhase { get; init; }

    public static LaunchResult CreateSuccess(int processId)
    {
        return new LaunchResult
        {
            Success = true,
            ProcessId = processId
        };
    }

    public static LaunchResult CreateFailed(LaunchPhase phase, string? errorMessage)
    {
        return new LaunchResult
        {
            Success = false,
            FailedPhase = phase,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Phases of the launch process.
/// </summary>
public enum LaunchPhase
{
    None,
    PipeCreation,
    ProcessCreation,
    DllInjection,
    ThreadResume,
    PipeConnection
}
