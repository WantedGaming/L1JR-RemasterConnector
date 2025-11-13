using LineageLauncher.Core.Entities;
using LineageLauncher.Core.Interfaces;
using LineageLauncher.Crypto;
using LineageLauncher.Launcher;
using LineageLauncher.Network;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Infrastructure;

/// <summary>
/// Real authentication service that authenticates users via the L1JR-Server connector API.
/// </summary>
public sealed class ConnectorAuthenticationService : IAuthenticationService
{
    private readonly ConnectorApiClient _connectorClient;
    private readonly HardwareIdCollector? _hardwareIdCollector;
    private readonly ILogger<ConnectorAuthenticationService> _logger;
    private string? _currentAuthToken;

    public ConnectorAuthenticationService(
        ConnectorApiClient connectorClient,
        ILogger<ConnectorAuthenticationService> logger,
        HardwareIdCollector? hardwareIdCollector = null)
    {
        _connectorClient = connectorClient ?? throw new ArgumentNullException(nameof(connectorClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hardwareIdCollector = hardwareIdCollector; // Optional on non-Windows platforms
    }

    /// <summary>
    /// Authenticates a user with the L1JR-Server via /outgame/login endpoint.
    /// </summary>
    public async Task<User?> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Authenticating user: {Username}", username);

            // Collect hardware IDs (Windows-only)
            string macAddress = string.Empty;
            string hddId = string.Empty;
            string boardId = string.Empty;
            string nicId = string.Empty;
            string runningProcesses = string.Empty;

            if (_hardwareIdCollector != null)
            {
                _logger.LogDebug("Collecting hardware IDs for anti-cheat validation");

#pragma warning disable CA1416 // Platform compatibility - HardwareIdCollector is optional and null-checked
                macAddress = _hardwareIdCollector.GetMacAddress();
                hddId = _hardwareIdCollector.GetHardDriveId();
                boardId = _hardwareIdCollector.GetMotherboardId();
                nicId = _hardwareIdCollector.GetNetworkInterfaceId();
                runningProcesses = _hardwareIdCollector.GetRunningProcesses();
#pragma warning restore CA1416

                // Log collected hardware IDs for debugging
                _logger.LogInformation("Hardware IDs collected:");
                _logger.LogInformation("  MAC Address (hashed): {MacAddress}", string.IsNullOrEmpty(macAddress) ? "[EMPTY]" : macAddress);
                _logger.LogInformation("  HDD ID (hashed): {HddId}", string.IsNullOrEmpty(hddId) ? "[EMPTY]" : hddId);
                _logger.LogInformation("  Board ID (hashed): {BoardId}", string.IsNullOrEmpty(boardId) ? "[EMPTY]" : boardId);
                _logger.LogInformation("  NIC ID (hashed): {NicId}", string.IsNullOrEmpty(nicId) ? "[EMPTY]" : nicId);

                // Warn if any critical IDs are empty
                if (string.IsNullOrEmpty(macAddress) || string.IsNullOrEmpty(hddId))
                {
                    _logger.LogWarning("CRITICAL: MAC Address or HDD ID is empty! HMAC validation will fail.");
                    _logger.LogWarning("This may be due to insufficient permissions. Try running launcher as Administrator.");
                }
            }
            else
            {
                _logger.LogWarning("HardwareIdCollector not available (non-Windows platform)");
            }

            // Calculate HMAC for mac_info parameter
            const string loginPath = "/outgame/login";
            var macInfo = HmacCalculator.CalculateMacInfo(hddId, macAddress, loginPath);

            _logger.LogDebug("Sending authentication request to server");
            _logger.LogDebug($"Account: {username}");
            _logger.LogDebug($"Password: {password}");
            _logger.LogDebug($"MAC Address: {macAddress}");
            _logger.LogDebug($"HDD ID: {hddId}");
            _logger.LogDebug($"Board ID: {boardId}");
            _logger.LogDebug($"NIC ID: {nicId}");
            _logger.LogDebug($"MAC Info: {macInfo}");

            // Authenticate with server
            var authToken = await _connectorClient.LoginAsync(
                username,
                password,
                macAddress,
                hddId,
                boardId,
                nicId,
                runningProcesses,
                macInfo,
                cancellationToken);

            _currentAuthToken = authToken;

            _logger.LogInformation("Authentication successful for user: {Username}", username);

            // Create and return User object with auth token
            return new User
            {
                Username = username,
                AuthToken = authToken,
                IsAuthenticated = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed for user: {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// Validates a stored authentication token.
    /// </summary>
    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // For now, just check if token matches the current auth token
        // TODO: Implement server-side token validation endpoint
        return Task.FromResult(!string.IsNullOrEmpty(token) && token == _currentAuthToken);
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    public Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        _currentAuthToken = null;
        _logger.LogInformation("User signed out");
        return Task.CompletedTask;
    }
}
