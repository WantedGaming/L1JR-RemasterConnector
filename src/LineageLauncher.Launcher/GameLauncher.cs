using System.Diagnostics;
using System.Text;
using LineageLauncher.Core.Entities;
using LineageLauncher.Core.Interfaces;

namespace LineageLauncher.Launcher;

/// <summary>
/// Implements process management for launching Lin.bin with proper parameter injection.
/// Based on L1R Lineage client requirements and server connector specifications.
/// </summary>
public sealed class GameLauncher : ILauncherService
{
    private System.Diagnostics.Process? _gameProcess;
    private readonly string _clientBasePath;
    private readonly bool _use64Bit;

    /// <summary>
    /// Initializes a new instance of the GameLauncher class.
    /// </summary>
    /// <param name="clientBasePath">Full path to the L1R-Client directory (e.g., "D:\L1R Project\L1R-Client")</param>
    /// <param name="use64Bit">Whether to use the 64-bit client (bin64\Lin.bin) instead of 32-bit (bin32\Lin.bin)</param>
    public GameLauncher(string clientBasePath, bool use64Bit = false)
    {
        _clientBasePath = clientBasePath ?? throw new ArgumentNullException(nameof(clientBasePath));
        _use64Bit = use64Bit;
    }

    /// <summary>
    /// Launches the Lineage game client with proper authentication parameters.
    /// </summary>
    public async Task<int> LaunchGameAsync(
        ServerInfo server,
        User user,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate client installation
        ValidateClientInstallation();

        // 2. Create Lineage.ini configuration file with server connection settings
        // Lin.bin reads server IP and port from this file, not from command-line arguments
        CreateLineageIniFile(server);

        // 3. Get authentication token from server (for future connector mode)
        // TODO: Integrate with L1JRApiClient for real authentication
        // For now, use placeholder until server API is available
        string authToken = await GetAuthTokenAsync(server, user, cancellationToken);

        // 4. Build Lin.bin path
        string binFolder = _use64Bit ? "bin64" : "bin32";
        string linBinPath = Path.Combine(_clientBasePath, binFolder, "Lin.bin");

        // 5. Configure process start info
        // NOTE: For direct client mode (LOGIN_TYPE=false), Lin.bin reads Lineage.ini
        //       Command-line arguments are only used in connector mode (LOGIN_TYPE=true)
        var startInfo = new ProcessStartInfo
        {
            FileName = linBinPath,
            Arguments = string.Empty,  // No arguments needed for direct client mode
            WorkingDirectory = _clientBasePath,  // CRITICAL: Must be client root, not bin32/bin64
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        // 6. Launch the game process
        _gameProcess = System.Diagnostics.Process.Start(startInfo);

        if (_gameProcess == null)
        {
            throw new InvalidOperationException($"Failed to start Lin.bin process at: {linBinPath}");
        }

        // 7. Enable process monitoring
        _gameProcess.EnableRaisingEvents = true;
        _gameProcess.Exited += OnGameProcessExited;

        Console.WriteLine($"[GameLauncher] Lin.bin launched successfully (PID: {_gameProcess.Id})");
        Console.WriteLine($"[GameLauncher] Connecting to server: {server.ServerAddress}:{server.ServerPort}");

        return _gameProcess.Id;
    }

    /// <summary>
    /// Checks if the game client is currently running.
    /// </summary>
    public bool IsGameRunning()
    {
        if (_gameProcess == null)
            return false;

        try
        {
            // Refresh process state to get current status
            _gameProcess.Refresh();
            return !_gameProcess.HasExited;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Terminates the running game client gracefully if possible, or forcefully if necessary.
    /// </summary>
    public async Task TerminateGameAsync(CancellationToken cancellationToken = default)
    {
        if (_gameProcess == null || _gameProcess.HasExited)
            return;

        try
        {
            // Try graceful shutdown first
            _gameProcess.CloseMainWindow();

            // Wait up to 5 seconds for graceful exit
            bool exited = await Task.Run(() =>
                _gameProcess.WaitForExit(5000),
                cancellationToken
            );

            // Force kill if still running after grace period
            if (!exited && !_gameProcess.HasExited)
            {
                _gameProcess.Kill();
                await Task.Run(() =>
                    _gameProcess.WaitForExit(),
                    cancellationToken
                );
            }
        }
        finally
        {
            _gameProcess?.Dispose();
            _gameProcess = null;
        }
    }

    /// <summary>
    /// Validates that the client installation is complete and all required files exist.
    /// </summary>
    private void ValidateClientInstallation()
    {
        if (!Directory.Exists(_clientBasePath))
        {
            throw new DirectoryNotFoundException(
                $"Client installation not found at: {_clientBasePath}");
        }

        // Check Lin.bin exists
        string binFolder = _use64Bit ? "bin64" : "bin32";
        string linBinPath = Path.Combine(_clientBasePath, binFolder, "Lin.bin");

        if (!File.Exists(linBinPath))
        {
            throw new FileNotFoundException(
                $"Lin.bin not found at: {linBinPath}\n" +
                $"Expected location: {_clientBasePath}\\{binFolder}\\Lin.bin");
        }

        // Verify critical PAK files exist
        string[] requiredFiles = new[]
        {
            "data.pak", "data.idx",
            "Icon.pak", "Icon.idx",
            "ui.pak", "ui.idx"
        };

        foreach (var file in requiredFiles)
        {
            string filePath = Path.Combine(_clientBasePath, file);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Critical client file missing: {file}\n" +
                    $"Expected at: {filePath}");
            }
        }
    }

    /// <summary>
    /// Creates or updates the Lineage.ini configuration file with server connection settings.
    /// Lin.bin reads this file to determine which server to connect to.
    /// This is required for direct client mode (LOGIN_TYPE=false on server).
    /// </summary>
    private void CreateLineageIniFile(ServerInfo server)
    {
        string binFolder = _use64Bit ? "bin64" : "bin32";
        string iniPath = Path.Combine(_clientBasePath, binFolder, "Lineage.ini");

        // Create INI file content with server connection settings
        var iniContent = new StringBuilder();
        iniContent.AppendLine("[Network]");
        iniContent.AppendLine($"ServerIP={server.ServerAddress}");
        iniContent.AppendLine($"ServerPort={server.ServerPort}");
        iniContent.AppendLine();
        iniContent.AppendLine("[Config]");
        iniContent.AppendLine("; Additional client configuration can go here");

        // Write the INI file
        try
        {
            File.WriteAllText(iniPath, iniContent.ToString());
            Console.WriteLine($"[GameLauncher] Created Lineage.ini at: {iniPath}");
            Console.WriteLine($"[GameLauncher] Server: {server.ServerAddress}:{server.ServerPort}");
        }
        catch (Exception ex)
        {
            throw new IOException(
                $"Failed to create Lineage.ini at: {iniPath}\n" +
                $"Error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves authentication token from the L1J server.
    /// </summary>
    private async Task<string> GetAuthTokenAsync(
        ServerInfo server,
        User user,
        CancellationToken cancellationToken)
    {
        // TODO: Implement real authentication with LineageLauncher.Network.L1JRApiClient
        // The server endpoint is: POST /api/account/login
        // Expected payload:
        // {
        //   "username": "user",
        //   "password_hash": "sha256_hash",
        //   "session_token": "generated_session",
        //   "launcher_version": "1.0.0"
        // }
        //
        // Returns: { "success": true, "auth_token": "base64_token", ... }

        // For now, return placeholder token for testing
        // This allows the launcher to start the client for UI testing
        await Task.Delay(100, cancellationToken); // Simulate network call

        // Generate a placeholder token (timestamp:username:random)
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string random = Guid.NewGuid().ToString("N")[..8];
        string tokenData = $"{timestamp}:{user.Username}:{random}";
        string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenData));

        return base64Token;
    }

    /// <summary>
    /// Builds command-line arguments for Lin.bin based on Lineage connector specifications.
    /// </summary>
    private string BuildLaunchArguments(string authToken, ServerInfo server)
    {
        // Build command-line parameters as documented in wiki.md and connector.properties
        // Format: /Key=Value /Key2=Value2 ...
        var args = new Dictionary<string, string>
        {
            { "AuthnToken", authToken },
            { "SessKey", authToken },           // Same as AuthnToken
            { "ServiceRegion", "NCS" },         // Always "NCS" for Korean Lineage
            { "AuthProviderCode", "np" },       // Always "np" (NC Provider)
            { "ServiceNetwork", "Live" },       // Always "Live"
            { "NPServerAddr", "https://api.ncsoft.com:443" },  // NC API endpoint
            { "UidHash", "798363913" },         // Fixed value
            { "PresenceId", "L1_KOR:3C164CEB-D15F-E011-9A06-E61F135E992F" },  // Fixed value
            { "UserAge", "10" }                 // Fixed value (age rating)
        };

        // Format parameters as command-line arguments
        var sb = new StringBuilder();
        foreach (var kvp in args)
        {
            sb.Append($"/{kvp.Key}={kvp.Value} ");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Event handler for when the game process exits.
    /// </summary>
    private void OnGameProcessExited(object? sender, EventArgs e)
    {
        if (_gameProcess == null)
            return;

        int exitCode = _gameProcess.ExitCode;

        // Log exit information (in production, use proper logging framework)
        Console.WriteLine($"[GameLauncher] Lin.bin exited with code: {exitCode}");

        if (exitCode != 0)
        {
            Console.WriteLine($"[GameLauncher] Non-zero exit code may indicate crash or error");
        }

        _gameProcess.Dispose();
        _gameProcess = null;
    }
}
