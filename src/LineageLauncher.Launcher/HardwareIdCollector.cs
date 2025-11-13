using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher;

/// <summary>
/// Collects hardware IDs for anti-cheat validation.
/// Windows-only implementation.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class HardwareIdCollector
{
    private readonly ILogger<HardwareIdCollector> _logger;

    public HardwareIdCollector(ILogger<HardwareIdCollector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets MAC address of the first active network interface.
    /// </summary>
    public string GetMacAddress()
    {
        try
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up
                    && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (nic != null)
            {
                var macAddress = nic.GetPhysicalAddress().ToString();
                _logger.LogDebug("MAC Address: {MacAddress}", macAddress);
                return HashValue(macAddress);
            }

            return HashValue("00:00:00:00:00:00");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get MAC address");
            return HashValue("00:00:00:00:00:00");
        }
    }

    /// <summary>
    /// Gets hard drive serial number.
    /// </summary>
    public string GetHardDriveId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
            var collection = searcher.Get();

            foreach (ManagementObject drive in collection)
            {
                var serialNumber = drive["SerialNumber"]?.ToString();
                if (!string.IsNullOrWhiteSpace(serialNumber))
                {
                    _logger.LogDebug("HDD Serial: {Serial}", serialNumber);
                    return HashValue(serialNumber);
                }
            }

            return HashValue("UNKNOWN_HDD");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get HDD ID");
            return HashValue("UNKNOWN_HDD");
        }
    }

    /// <summary>
    /// Gets motherboard serial number.
    /// </summary>
    public string GetMotherboardId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            var collection = searcher.Get();

            foreach (ManagementObject board in collection)
            {
                var serialNumber = board["SerialNumber"]?.ToString();
                if (!string.IsNullOrWhiteSpace(serialNumber))
                {
                    _logger.LogDebug("Board Serial: {Serial}", serialNumber);
                    return HashValue(serialNumber);
                }
            }

            return HashValue("UNKNOWN_BOARD");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get motherboard ID");
            return HashValue("UNKNOWN_BOARD");
        }
    }

    /// <summary>
    /// Gets network interface card ID.
    /// </summary>
    public string GetNetworkInterfaceId()
    {
        try
        {
            var nic = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up
                    && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (nic != null)
            {
                var nicId = nic.Id;
                _logger.LogDebug("NIC ID: {NicId}", nicId);
                return HashValue(nicId);
            }

            return HashValue("UNKNOWN_NIC");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get NIC ID");
            return HashValue("UNKNOWN_NIC");
        }
    }

    /// <summary>
    /// Gets list of running processes (comma-separated).
    /// </summary>
    public string GetRunningProcesses()
    {
        try
        {
            var processes = System.Diagnostics.Process.GetProcesses()
                .Select(p => p.ProcessName)
                .Take(50)  // Limit to first 50 processes
                .ToArray();

            return string.Join(",", processes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get running processes");
            return string.Empty;
        }
    }

    /// <summary>
    /// Hashes a value using SHA256 for privacy.
    /// </summary>
    private static string HashValue(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
