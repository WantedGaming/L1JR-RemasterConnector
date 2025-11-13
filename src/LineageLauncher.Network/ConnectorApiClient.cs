using System.Net.Http.Json;
using LineageLauncher.Core.Entities;
using LineageLauncher.Crypto;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace LineageLauncher.Network;

/// <summary>
/// HTTP API client for L1JR-Server connector endpoints.
/// </summary>
public sealed class ConnectorApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConnectorApiClient> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private const string CONNECTOR_ENCRYPT_KEY = "mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=";

    public ConnectorApiClient(HttpClient httpClient, ILogger<ConnectorApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure retry policy
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .Build();
    }

    /// <summary>
    /// Gets connector configuration from /outgame/info endpoint.
    /// </summary>
    public async Task<DecryptedConnectorInfo> GetConnectorInfoAsync(CancellationToken cancellationToken = default)
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
        {
            _logger.LogDebug("Fetching connector info from /outgame/info");
            var response = await _httpClient.GetAsync("/outgame/info", ct);
            response.EnsureSuccessStatusCode();

            // Read raw JSON for debugging
            var jsonContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("Received connector info JSON (length: {Length})", jsonContent.Length);
            _logger.LogDebug("JSON content: {Json}", jsonContent);

            // Deserialize from string
            var encryptedInfo = System.Text.Json.JsonSerializer.Deserialize<ConnectorInfo>(jsonContent);
            if (encryptedInfo == null)
            {
                _logger.LogError("Failed to deserialize connector info - result was null");
                throw new InvalidOperationException("Failed to deserialize connector info response.");
            }

            _logger.LogDebug("Successfully deserialized ConnectorInfo");
            _logger.LogDebug("Decrypting connector info...");
            var decrypted = DecryptConnectorInfo(encryptedInfo);
            _logger.LogInformation("Connector info decrypted successfully: ServerIp={ServerIp}, ServerPort={ServerPort}",
                decrypted.ServerIp, decrypted.ServerPort);
            return decrypted;
        }, cancellationToken);
    }

    /// <summary>
    /// Authenticates via /outgame/login endpoint and returns auth token.
    /// </summary>
    public async Task<string> LoginAsync(
        string account,
        string password,
        string macAddress,
        string hddId,
        string boardId,
        string nicId,
        string process,
        string macInfo,
        CancellationToken cancellationToken = default)
    {
        // Server expects parameters in URL query string, not POST body
        var queryParams = new Dictionary<string, string>
        {
            ["account"] = account,
            ["password"] = password,
            ["mac_address"] = macAddress,
            ["hdd_id"] = hddId,
            ["board_id"] = boardId,
            ["nic_id"] = nicId,
            ["process"] = process,
            ["mac_info"] = macInfo
        };

        // Build query string
        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var response = await _httpClient.PostAsync($"/outgame/login?{queryString}", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException("Invalid response from server.");
        }

        if (result.ResultCode != "SUCCESS")
        {
            throw new InvalidOperationException($"Login failed: {result.ResultCode}");
        }

        return result.AuthToken ?? throw new InvalidOperationException("No auth token received.");
    }

    /// <summary>
    /// Creates a new account via /outgame/accountcreate endpoint.
    /// </summary>
    public async Task<string> CreateAccountAsync(
        string account,
        string password,
        string phone,
        string macAddress,
        string hddId,
        string boardId,
        string nicId,
        string macInfo,
        CancellationToken cancellationToken = default)
    {
        // Server expects parameters in URL query string, not POST body
        var queryParams = new Dictionary<string, string>
        {
            ["account"] = account,
            ["password"] = password,
            ["phone"] = phone,
            ["mac_address"] = macAddress,
            ["hdd_id"] = hddId,
            ["board_id"] = boardId,
            ["nic_id"] = nicId,
            ["mac_info"] = macInfo
        };

        // Build query string
        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var response = await _httpClient.PostAsync($"/outgame/accountcreate?{queryString}", null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateAccountResponse>(cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException("Invalid response from server.");
        }

        return result.ResultCode;
    }

    /// <summary>
    /// Downloads a file from the connector.
    /// </summary>
    public async Task<byte[]> DownloadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Decrypts connector info response.
    /// </summary>
    private static DecryptedConnectorInfo DecryptConnectorInfo(ConnectorInfo encrypted)
    {
        return new DecryptedConnectorInfo
        {
            ServerIp = Base64Crypto.DecryptFromBase64(encrypted.ServerIp, CONNECTOR_ENCRYPT_KEY),
            ServerPort = Base64Crypto.DecryptInt(encrypted.ServerPort, CONNECTOR_ENCRYPT_KEY),
            BrowserUrl = Base64Crypto.DecryptFromBase64(encrypted.BrowserUrl, CONNECTOR_ENCRYPT_KEY),

            LinbinPath = Base64Crypto.DecryptFromBase64(encrypted.Linbin, CONNECTOR_ENCRYPT_KEY),
            LinbinSize = Base64Crypto.DecryptInt(encrypted.LinbinSize, CONNECTOR_ENCRYPT_KEY),
            LinbinVersion = Base64Crypto.DecryptFromBase64(encrypted.LinbinVersion, CONNECTOR_ENCRYPT_KEY),

            MsdllPath = Base64Crypto.DecryptFromBase64(encrypted.Msdll, CONNECTOR_ENCRYPT_KEY),
            MsdllSize = Base64Crypto.DecryptInt(encrypted.MsdllSize, CONNECTOR_ENCRYPT_KEY),
            MsdllVersion = Base64Crypto.DecryptFromBase64(encrypted.MsdllVersion, CONNECTOR_ENCRYPT_KEY),

            LibcocosPath = Base64Crypto.DecryptFromBase64(encrypted.Libcocos, CONNECTOR_ENCRYPT_KEY),
            LibcocosSize = Base64Crypto.DecryptInt(encrypted.LibcocosSize, CONNECTOR_ENCRYPT_KEY),
            LibcocosVersion = Base64Crypto.DecryptFromBase64(encrypted.LibcocosVersion, CONNECTOR_ENCRYPT_KEY),

            BoxdllPath = Base64Crypto.DecryptFromBase64(encrypted.Boxdll, CONNECTOR_ENCRYPT_KEY),
            PatchPath = Base64Crypto.DecryptFromBase64(encrypted.Patch, CONNECTOR_ENCRYPT_KEY),
            PatchVersion = Base64Crypto.DecryptInt(encrypted.PatchVersion, CONNECTOR_ENCRYPT_KEY),

            ClientSideKey = Base64Crypto.DecryptInt(encrypted.ClientSideKey, CONNECTOR_ENCRYPT_KEY),
            DllPassword = Base64Crypto.DecryptInt(encrypted.DllPassword, CONNECTOR_ENCRYPT_KEY)
        };
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Response from /outgame/login endpoint.
/// </summary>
public sealed class LoginResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("result_code")]
    public required string ResultCode { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("auth_token")]
    public string? AuthToken { get; init; }
}

/// <summary>
/// Response from /outgame/accountcreate endpoint.
/// </summary>
public sealed class CreateAccountResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("result_code")]
    public required string ResultCode { get; init; }
}
