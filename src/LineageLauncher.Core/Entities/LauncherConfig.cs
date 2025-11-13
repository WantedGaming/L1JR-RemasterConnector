namespace LineageLauncher.Core.Entities;

/// <summary>
/// Represents the launcher's configuration settings.
/// </summary>
public sealed class LauncherConfig
{
    /// <summary>
    /// Full path to the L1R-Client directory (e.g., "D:\L1R Project\L1R-Client")
    /// </summary>
    public required string GameInstallPath { get; init; }

    /// <summary>
    /// Whether to use the 64-bit client (bin64\Lin.bin) instead of 32-bit (bin32\Lin.bin)
    /// </summary>
    public bool Use64BitClient { get; init; } = false;

    /// <summary>
    /// Client version string (e.g., "2303281701")
    /// </summary>
    public string ClientVersion { get; init; } = "2303281701";

    public required ServerInfo Server { get; init; }
    public LauncherSettings Settings { get; init; } = new();
}

/// <summary>
/// User-configurable launcher settings.
/// </summary>
public sealed class LauncherSettings
{
    public bool AutoPatch { get; init; } = true;
    public bool CheckIntegrity { get; init; } = true;
    public bool MinimizeOnLaunch { get; init; } = true;
    public int MaxDownloadThreads { get; init; } = 4;
    public string Language { get; init; } = "en-US";
}
