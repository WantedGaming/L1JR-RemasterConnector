namespace LineageLauncher.Core.Entities;

/// <summary>
/// Represents server configuration and connection information.
/// </summary>
public sealed class ServerInfo
{
    public required string ServerName { get; init; }
    public required string ServerAddress { get; init; }
    public required int ServerPort { get; init; }
    public required string ApiBaseUrl { get; init; }
    public ServerStatus Status { get; init; } = ServerStatus.Unknown;
    public int OnlinePlayerCount { get; init; }
    public string? MessageOfTheDay { get; init; }

    // Client configuration (from connector API or appsettings.json)
    public string? ClientPath { get; init; }
    public int? DllPassword { get; init; }
    public int? ClientSideKey { get; init; }
}

public enum ServerStatus
{
    Unknown,
    Online,
    Offline,
    Maintenance
}
