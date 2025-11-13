using LineageLauncher.Core.Entities;

namespace LineageLauncher.Core.Interfaces;

/// <summary>
/// Defines the contract for game patching operations.
/// </summary>
public interface IPatchService
{
    /// <summary>
    /// Checks if patches are available for the local game installation.
    /// </summary>
    Task<PatchManifest?> CheckForUpdatesAsync(string localVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads and applies patches from the manifest.
    /// </summary>
    /// <param name="manifest">The patch manifest to apply.</param>
    /// <param name="progress">Progress callback (current file index, total files, bytes downloaded, total bytes).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ApplyPatchAsync(PatchManifest manifest, IProgress<PatchProgress>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the integrity of local game files.
    /// </summary>
    Task<IReadOnlyList<string>> VerifyIntegrityAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents progress information for a patch operation.
/// </summary>
public sealed class PatchProgress
{
    public required int CurrentFileIndex { get; init; }
    public required int TotalFiles { get; init; }
    public required long BytesDownloaded { get; init; }
    public required long TotalBytes { get; init; }
    public required string CurrentFileName { get; init; }
    public double PercentComplete => TotalBytes > 0 ? (double)BytesDownloaded / TotalBytes * 100 : 0;
}
