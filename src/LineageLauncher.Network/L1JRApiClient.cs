using System.Net.Http.Json;
using LineageLauncher.Core.Entities;
using Polly;
using Polly.Retry;

namespace LineageLauncher.Network;

/// <summary>
/// HTTP API client for communicating with the L1JR-Server.
/// </summary>
public sealed class L1JRApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline _retryPipeline;

    public L1JRApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Configure retry policy with exponential backoff
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
    /// Retrieves the current server status and information.
    /// </summary>
    public async Task<ServerInfo?> GetServerStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
        {
            var response = await _httpClient.GetAsync("api/launcher/status", ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ServerInfo>(ct);
        }, cancellationToken);
    }

    /// <summary>
    /// Retrieves the latest patch manifest for client updates.
    /// </summary>
    public async Task<PatchManifest?> GetPatchManifestAsync(string currentVersion, CancellationToken cancellationToken = default)
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
        {
            var response = await _httpClient.GetAsync($"api/launcher/patches?version={currentVersion}", ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PatchManifest>(ct);
        }, cancellationToken);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
