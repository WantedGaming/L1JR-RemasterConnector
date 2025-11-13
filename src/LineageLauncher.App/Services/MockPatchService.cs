using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineageLauncher.Core.Entities;
using LineageLauncher.Core.Interfaces;

namespace LineageLauncher.App.Services;

/// <summary>
/// Mock implementation of patch service for UI testing.
/// Simulates patch checking and downloading with progress updates.
/// </summary>
public class MockPatchService : IPatchService
{
    private readonly Random _random = new();

    public async Task<PatchManifest?> CheckForUpdatesAsync(
        string localVersion,
        CancellationToken cancellationToken = default)
    {
        // Simulate server communication delay
        await Task.Delay(1500, cancellationToken);

        // Randomly return patches or no patches (70% chance of patches)
        if (_random.Next(100) < 70)
        {
            // Return a mock manifest with patches
            return new PatchManifest
            {
                Version = "1.0.1",
                CreatedAt = DateTime.UtcNow,
                Files = new List<PatchFile>
                {
                    new()
                    {
                        RelativePath = "data.pak",
                        CompressedSize = 15_000_000,
                        UncompressedSize = 20_000_000,
                        Checksum = "abc123",
                        DownloadUrl = "http://localhost/patches/data.pak"
                    },
                    new()
                    {
                        RelativePath = "ui.pak",
                        CompressedSize = 25_000_000,
                        UncompressedSize = 30_000_000,
                        Checksum = "def456",
                        DownloadUrl = "http://localhost/patches/ui.pak"
                    },
                    new()
                    {
                        RelativePath = "Lin.bin",
                        CompressedSize = 5_000_000,
                        UncompressedSize = 7_000_000,
                        Checksum = "ghi789",
                        DownloadUrl = "http://localhost/patches/Lin.bin"
                    }
                }
            };
        }

        // No patches needed
        return null;
    }

    public async Task ApplyPatchAsync(
        PatchManifest manifest,
        IProgress<PatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (manifest?.Files == null || manifest.Files.Count == 0)
            return;

        long totalBytes = 0;
        foreach (var file in manifest.Files)
        {
            totalBytes += file.CompressedSize;
        }

        long bytesDownloaded = 0;
        int fileIndex = 0;

        foreach (var file in manifest.Files)
        {
            fileIndex++;

            // Simulate downloading in chunks
            const int chunkSize = 500_000; // 500 KB chunks
            long fileBytes = 0;

            while (fileBytes < file.CompressedSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Simulate download time per chunk
                await Task.Delay(200, cancellationToken);

                var downloadAmount = Math.Min(chunkSize, file.CompressedSize - fileBytes);
                fileBytes += downloadAmount;
                bytesDownloaded += downloadAmount;

                // Report progress
                progress?.Report(new PatchProgress
                {
                    CurrentFileIndex = fileIndex,
                    TotalFiles = manifest.Files.Count,
                    BytesDownloaded = bytesDownloaded,
                    TotalBytes = totalBytes,
                    CurrentFileName = file.RelativePath
                });
            }
        }

        // Simulate final installation delay
        await Task.Delay(500, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> VerifyIntegrityAsync(
        CancellationToken cancellationToken = default)
    {
        // Simulate integrity check
        await Task.Delay(2000, cancellationToken);

        // Return empty list (no corrupted files in mock)
        return Array.Empty<string>();
    }
}
