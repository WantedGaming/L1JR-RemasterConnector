using System.IO.Compression;
using System.Security.Cryptography;
using LineageLauncher.Core.Entities;
using LineageLauncher.Core.Interfaces;
using LineageLauncher.Network;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher;

/// <summary>
/// Service for downloading and deploying required DLL files.
/// </summary>
public sealed class DllDeploymentService : IDllDeploymentService
{
    private readonly ConnectorApiClient _connectorClient;
    private readonly ILogger<DllDeploymentService> _logger;
    private DecryptedConnectorInfo? _connectorInfo;

    public DllDeploymentService(
        ConnectorApiClient connectorClient,
        ILogger<DllDeploymentService> logger)
    {
        _connectorClient = connectorClient ?? throw new ArgumentNullException(nameof(connectorClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> DeployDllsAsync(
        string clientDirectory,
        IProgress<(string fileName, int percentage)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting DLL deployment to {ClientDirectory}", clientDirectory);

            // Get connector configuration
            _connectorInfo = await _connectorClient.GetConnectorInfoAsync(cancellationToken);

            // Download MS_DLL (210916.asi)
            progress?.Report(("MS_DLL (210916.asi)", 0));
            await DownloadDllAsync(
                _connectorInfo.MsdllPath,
                "210916.asi",
                _connectorInfo.MsdllSize,
                clientDirectory,
                progress,
                cancellationToken);

            // Download boxer.dll
            progress?.Report(("boxer.dll", 33));
            await DownloadDllAsync(
                _connectorInfo.BoxdllPath,
                "boxer.dll",
                null,  // Size not validated for boxer
                clientDirectory,
                progress,
                cancellationToken);

            // Download and extract libcocos2d.zip
            progress?.Report(("libcocos2d.zip", 66));
            await DownloadAndExtractLibcocosAsync(
                _connectorInfo.LibcocosPath,
                _connectorInfo.LibcocosSize,
                clientDirectory,
                progress,
                cancellationToken);

            progress?.Report(("Deployment complete", 100));
            _logger.LogInformation("DLL deployment completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deploy DLLs");
            return false;
        }
    }

    public Task<bool> ValidateDllsAsync(string clientDirectory)
    {
        try
        {
            var requiredFiles = new[]
            {
                "210916.asi",
                "boxer.dll",
                "libcocos2d.dll"
            };

            foreach (var file in requiredFiles)
            {
                var filePath = Path.Combine(clientDirectory, file);
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Required DLL not found: {FilePath}", filePath);
                    return Task.FromResult(false);
                }
            }

            _logger.LogInformation("All required DLLs validated successfully");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate DLLs");
            return Task.FromResult(false);
        }
    }

    private async Task DownloadDllAsync(
        string relativePath,
        string fileName,
        int? expectedSize,
        string clientDirectory,
        IProgress<(string fileName, int percentage)>? progress,
        CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(clientDirectory, fileName);

        // Check if file already exists
        if (File.Exists(filePath))
        {
            _logger.LogInformation("File {FileName} already exists, checking hash", fileName);

            try
            {
                var existingData = await File.ReadAllBytesAsync(filePath, cancellationToken);
                var existingHash = CalculateSHA256(existingData);

                // Download file to compare hashes
                _logger.LogInformation("Downloading {FileName} from {Path} for hash comparison", fileName, relativePath);
                var fileData = await _connectorClient.DownloadFileAsync(relativePath, cancellationToken);

                // Validate size if provided
                if (expectedSize.HasValue && fileData.Length != expectedSize.Value)
                {
                    throw new InvalidOperationException(
                        $"Downloaded file size mismatch for {fileName}. Expected: {expectedSize.Value}, Got: {fileData.Length}");
                }

                var expectedHash = CalculateSHA256(fileData);

                if (existingHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "File {FileName} matches expected hash ({Hash}), skipping download",
                        fileName,
                        expectedHash);
                    return;
                }
                else
                {
                    _logger.LogWarning(
                        "File {FileName} hash mismatch. Expected: {Expected}, Actual: {Actual}, will overwrite",
                        fileName,
                        expectedHash,
                        existingHash);

                    // Remove read-only attribute if set
                    var attributes = File.GetAttributes(filePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        _logger.LogInformation("Removing read-only attribute from {FileName}", fileName);
                        File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                    }

                    // Write the new file
                    await File.WriteAllBytesAsync(filePath, fileData, cancellationToken);
                    _logger.LogInformation("Successfully updated {FileName} ({Size} bytes)", fileName, fileData.Length);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied when checking or updating {FileName}, skipping", fileName);
                // Continue anyway - file exists, even if we can't verify/update it
                return;
            }
        }
        else
        {
            // File doesn't exist, download normally
            _logger.LogInformation("Downloading {FileName} from {Path}", fileName, relativePath);

            var fileData = await _connectorClient.DownloadFileAsync(relativePath, cancellationToken);

            // Validate size if provided
            if (expectedSize.HasValue && fileData.Length != expectedSize.Value)
            {
                throw new InvalidOperationException(
                    $"Downloaded file size mismatch for {fileName}. Expected: {expectedSize.Value}, Got: {fileData.Length}");
            }

            // Calculate and log SHA256 hash
            var hash = CalculateSHA256(fileData);
            _logger.LogInformation("File {FileName} SHA256: {Hash}", fileName, hash);

            await File.WriteAllBytesAsync(filePath, fileData, cancellationToken);

            _logger.LogInformation("Successfully downloaded {FileName} ({Size} bytes)", fileName, fileData.Length);
        }
    }

    /// <summary>
    /// Calculates SHA256 hash of byte array.
    /// </summary>
    private static string CalculateSHA256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(data);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Validates file SHA256 hash against expected value.
    /// </summary>
    private bool ValidateSHA256(byte[] data, string expectedHash)
    {
        if (string.IsNullOrEmpty(expectedHash))
        {
            _logger.LogWarning("No expected hash provided for validation");
            return true; // Skip validation if no hash provided
        }

        var actualHash = CalculateSHA256(data);
        var isValid = actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            _logger.LogError(
                "SHA256 hash mismatch! Expected: {Expected}, Actual: {Actual}",
                expectedHash,
                actualHash);
        }

        return isValid;
    }

    private async Task DownloadAndExtractLibcocosAsync(
        string relativePath,
        int expectedSize,
        string clientDirectory,
        IProgress<(string fileName, int percentage)>? progress,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading and extracting libcocos2d.zip");

        var zipData = await _connectorClient.DownloadFileAsync(relativePath, cancellationToken);

        if (zipData.Length != expectedSize)
        {
            throw new InvalidOperationException(
                $"Downloaded ZIP size mismatch. Expected: {expectedSize}, Got: {zipData.Length}");
        }

        var tempZipPath = Path.Combine(Path.GetTempPath(), "libcocos2d.zip");

        try
        {
            // Save ZIP to temp location
            await File.WriteAllBytesAsync(tempZipPath, zipData, cancellationToken);

            // Extract to client directory
            ZipFile.ExtractToDirectory(tempZipPath, clientDirectory, overwriteFiles: true);

            _logger.LogInformation("Successfully extracted libcocos2d.zip to {ClientDirectory}", clientDirectory);
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }
        }
    }
}
