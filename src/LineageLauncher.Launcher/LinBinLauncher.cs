using System.Diagnostics;
using System.Text;
using LineageLauncher.Core.Entities;
using LineageLauncher.Core.Interfaces;
using LineageLauncher.Crypto;
using LineageLauncher.Launcher.Orchestration;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher;

/// <summary>
/// Service for launching the Lineage client (Lin.bin) with DLL injection.
/// </summary>
public sealed class LinBinLauncher : ILauncherService
{
    private readonly ILogger<LinBinLauncher> _logger;
    private readonly ProcessLaunchOrchestrator _orchestrator;
    private System.Diagnostics.Process? _gameProcess;

    public LinBinLauncher(
        ILogger<LinBinLauncher> logger,
        ProcessLaunchOrchestrator orchestrator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    /// <summary>
    /// Launches the game with server and user information.
    /// Implementation of ILauncherService interface.
    /// </summary>
    public async Task<int> LaunchGameAsync(
        ServerInfo serverInfo,
        User user,
        CancellationToken cancellationToken = default)
    {
        // Build client path from serverInfo configuration
        var clientPath = serverInfo.ClientPath
            ?? throw new InvalidOperationException("Client path not configured in ServerInfo");

        var serverIp = serverInfo.ServerAddress ?? "127.0.0.1";
        var serverPort = serverInfo.ServerPort;

        // Get DLL passwords from serverInfo (retrieved from connector API)
        var dllPassword = serverInfo.DllPassword
            ?? throw new InvalidOperationException("DLL password not configured in ServerInfo");
        var clientSideKey = serverInfo.ClientSideKey
            ?? throw new InvalidOperationException("Client side key not configured in ServerInfo");

        var success = await LaunchGameInternalAsync(
            clientPath,
            serverIp,
            serverPort,
            dllPassword,
            clientSideKey,
            cancellationToken);

        return success && _gameProcess != null && !_gameProcess.HasExited ? _gameProcess.Id : -1;
    }

    /// <summary>
    /// Terminates the game process.
    /// Implementation of ILauncherService interface.
    /// </summary>
    public Task TerminateGameAsync(CancellationToken cancellationToken = default)
    {
        StopGame();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Launches Lin.bin with DLL injection and server connection parameters (internal method).
    /// </summary>
    private async Task<bool> LaunchGameInternalAsync(
        string clientPath,
        string serverIp,
        int serverPort,
        int dllPassword,
        int clientSideKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Launching Lin.bin with DLL injection: {ClientPath}, Server: {ServerIp}:{ServerPort}",
                clientPath,
                serverIp,
                serverPort);

            if (!File.Exists(clientPath))
            {
                throw new FileNotFoundException($"Lin.bin not found at: {clientPath}");
            }

            // Get bin directory (e.g., "C:\Lineage Warrior\bin64")
            var binDirectory = Path.GetDirectoryName(clientPath)
                ?? throw new InvalidOperationException("Could not determine bin directory");

            // Get client root directory (e.g., "C:\Lineage Warrior")
            // CRITICAL: Working directory must be client root, not bin32/bin64
            var clientRootDirectory = Path.GetDirectoryName(binDirectory)
                ?? throw new InvalidOperationException("Could not determine client root directory");

            _logger.LogInformation(
                "Client paths - Root: {Root}, Bin: {Bin}, Executable: {Exe}",
                clientRootDirectory,
                binDirectory,
                clientPath);

            // Create Login.ini for server connection (in client root)
            CreateLoginIni(clientRootDirectory, serverIp, serverPort);

            // Build DLL paths for injection (DLLs are in bin directory)
            var dllsToInject = new List<string>
            {
                Path.Combine(binDirectory, "bb64.dll"),
                Path.Combine(binDirectory, "bdcap64.dll"),
                Path.Combine(binDirectory, "libcocos2d.dll")
            };

            // Verify all DLLs exist
            foreach (var dll in dllsToInject.ToList())
            {
                if (!File.Exists(dll))
                {
                    _logger.LogWarning("DLL not found, removing from injection list: {Dll}", dll);
                    dllsToInject.Remove(dll);
                }
            }

            if (dllsToInject.Count == 0)
            {
                _logger.LogError("No DLLs found for injection in: {Directory}", binDirectory);
                return false;
            }

            _logger.LogInformation("Will inject {Count} DLLs: {Dlls}",
                dllsToInject.Count,
                string.Join(", ", dllsToInject.Select(Path.GetFileName)));

            // Build environment variables
            var envVars = new Dictionary<string, string>
            {
                ["L1_DLL_PASSWORD"] = dllPassword.ToString(),
                ["L1_CLIENT_SIDE_KEY"] = clientSideKey.ToString()
            };

            // Configure launch with DLL injection
            var config = new LaunchConfiguration
            {
                ExecutablePath = clientPath,
                WorkingDirectory = clientRootDirectory, // CRITICAL: Use client root, not bin directory
                DllsToInject = dllsToInject,
                CommandLine = null,
                EnvironmentVariables = envVars,
                PipeConnectionTimeout = TimeSpan.FromSeconds(10)
            };

            // Launch with orchestrator
            var result = await _orchestrator.LaunchWithInjectionAsync(
                config,
                progress: null,
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogError(
                    "Launch failed at phase {Phase}: {Error}",
                    result.FailedPhase,
                    result.ErrorMessage);
                return false;
            }

            _logger.LogInformation(
                "Game launched successfully with DLL injection. PID: {ProcessId}",
                result.ProcessId);

            // Store process for monitoring (optional)
            try
            {
                _gameProcess = System.Diagnostics.Process.GetProcessById(result.ProcessId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not attach to game process for monitoring");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch game with DLL injection");
            return false;
        }
    }

    /// <summary>
    /// Creates Login.ini configuration file for server connection.
    /// Lin.bin expects this file in the bin64 directory (same location as the executable).
    /// </summary>
    private void CreateLoginIni(string clientDirectory, string serverIp, int serverPort)
    {
        // Login.ini must be in the same directory as Lin.bin (bin64)
        var loginIniPath = Path.Combine(clientDirectory, "Login.ini");

        var content = new StringBuilder();
        content.AppendLine("[Server]");
        content.AppendLine($"ip={serverIp}");
        content.AppendLine($"port={serverPort}");
        content.AppendLine();
        content.AppendLine("[Options]");
        content.AppendLine("RememberUser=1");
        content.AppendLine("Language=1");

        File.WriteAllText(loginIniPath, content.ToString());

        _logger.LogInformation("Created Login.ini at {Path}", loginIniPath);
        _logger.LogInformation("Server configuration: {ServerIp}:{ServerPort}", serverIp, serverPort);
    }

    /// <summary>
    /// Checks if the game process is still running.
    /// </summary>
    public bool IsGameRunning()
    {
        return _gameProcess != null && !_gameProcess.HasExited;
    }

    /// <summary>
    /// Terminates the game process if running.
    /// </summary>
    public void StopGame()
    {
        if (_gameProcess != null && !_gameProcess.HasExited)
        {
            _logger.LogInformation("Terminating game process: {ProcessId}", _gameProcess.Id);
            _gameProcess.Kill();
            _gameProcess.Dispose();
            _gameProcess = null;
        }
    }
}
