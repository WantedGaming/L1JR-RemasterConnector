using System.Text.Json.Serialization;

namespace LineageLauncher.Core.Entities;

/// <summary>
/// Response from /api/connector/info endpoint (encrypted values).
/// All fields are Base64-encoded and XOR-encrypted with CONNECTOR_ENCRYPT_KEY.
/// </summary>
public sealed class ConnectorInfo
{
    // Server connection info
    [JsonPropertyName("serverIp")]
    public required string ServerIp { get; init; }

    [JsonPropertyName("serverPort")]
    public required string ServerPort { get; init; }

    [JsonPropertyName("browserUrl")]
    public required string BrowserUrl { get; init; }

    // Lin.bin info
    [JsonPropertyName("linbin")]
    public required string Linbin { get; init; }

    [JsonPropertyName("linbinSize")]
    public required string LinbinSize { get; init; }

    [JsonPropertyName("linbinVersion")]
    public required string LinbinVersion { get; init; }

    // MS_DLL (210916.asi) info
    [JsonPropertyName("msdll")]
    public required string Msdll { get; init; }

    [JsonPropertyName("msdllSize")]
    public required string MsdllSize { get; init; }

    [JsonPropertyName("msdllVersion")]
    public required string MsdllVersion { get; init; }

    // LIBCOCOS2D.DLL info
    [JsonPropertyName("libcocos")]
    public required string Libcocos { get; init; }

    [JsonPropertyName("libcocosSize")]
    public required string LibcocosSize { get; init; }

    [JsonPropertyName("libcocosVersion")]
    public required string LibcocosVersion { get; init; }

    // BOXER.DLL info
    [JsonPropertyName("boxdll")]
    public required string Boxdll { get; init; }

    // Patch info
    [JsonPropertyName("patch")]
    public required string Patch { get; init; }

    [JsonPropertyName("patchVersion")]
    public required string PatchVersion { get; init; }

    // Configuration keys
    [JsonPropertyName("clientSideKey")]
    public required string ClientSideKey { get; init; }

    [JsonPropertyName("dllPassword")]
    public required string DllPassword { get; init; }

    // Opcodes (for client compatibility)
    [JsonPropertyName("C_CHANNEL")]
    public required string C_CHANNEL { get; init; }

    [JsonPropertyName("C_LOGIN")]
    public required string C_LOGIN { get; init; }

    [JsonPropertyName("C_LOGOUT")]
    public required string C_LOGOUT { get; init; }

    [JsonPropertyName("S_KEY")]
    public required string S_KEY { get; init; }

    [JsonPropertyName("S_EXTENDED_PROTO_BUF")]
    public required string S_EXTENDED_PROTO_BUF { get; init; }

    [JsonPropertyName("C_EXTENDED_PROTO_BUF")]
    public required string C_EXTENDED_PROTO_BUF { get; init; }
}

/// <summary>
/// Decrypted connector configuration.
/// </summary>
public sealed class DecryptedConnectorInfo
{
    public required string ServerIp { get; init; }
    public required int ServerPort { get; init; }
    public required string BrowserUrl { get; init; }

    public required string LinbinPath { get; init; }
    public required int LinbinSize { get; init; }
    public required string LinbinVersion { get; init; }

    public required string MsdllPath { get; init; }
    public required int MsdllSize { get; init; }
    public required string MsdllVersion { get; init; }

    public required string LibcocosPath { get; init; }
    public required int LibcocosSize { get; init; }
    public required string LibcocosVersion { get; init; }

    public required string BoxdllPath { get; init; }
    public required string PatchPath { get; init; }
    public required int PatchVersion { get; init; }

    public required int ClientSideKey { get; init; }
    public required int DllPassword { get; init; }
}
