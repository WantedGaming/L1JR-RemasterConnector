using System.Security.Cryptography;
using System.Text;

namespace LineageLauncher.Crypto;

/// <summary>
/// Utility for calculating HMAC-SHA256 hashes for server authentication.
/// </summary>
public static class HmacCalculator
{
    private const string CONNECTOR_SESSION_KEY = "linoffice1234";

    /// <summary>
    /// Calculates the HMAC-SHA256 mac_info value required by the server.
    /// </summary>
    /// <param name="hddId">Hashed hard drive ID.</param>
    /// <param name="macAddress">Hashed MAC address.</param>
    /// <param name="path">API endpoint path (e.g., "/outgame/login").</param>
    /// <returns>Base64-encoded HMAC-SHA256 hash.</returns>
    public static string CalculateMacInfo(string hddId, string macAddress, string path)
    {
        try
        {
            // Format: {hdd_id}.{mac_address}@{path}
            var message = $"{hddId}.{macAddress}@{path}";

            // Log HMAC calculation details for debugging (use Console since this is a static class)
            Console.WriteLine($"[HMAC] Calculating mac_info:");
            Console.WriteLine($"[HMAC]   HDD ID: {(string.IsNullOrEmpty(hddId) ? "[EMPTY]" : hddId)}");
            Console.WriteLine($"[HMAC]   MAC Address: {(string.IsNullOrEmpty(macAddress) ? "[EMPTY]" : macAddress)}");
            Console.WriteLine($"[HMAC]   Path: {path}");
            Console.WriteLine($"[HMAC]   Message: {message}");
            Console.WriteLine($"[HMAC]   Key: {CONNECTOR_SESSION_KEY}");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(CONNECTOR_SESSION_KEY));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));

            var result = Convert.ToBase64String(hashBytes);
            Console.WriteLine($"[HMAC]   Result: {result}");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HMAC] ERROR: {ex.Message}");
            throw new InvalidOperationException("Failed to calculate HMAC", ex);
        }
    }
}
