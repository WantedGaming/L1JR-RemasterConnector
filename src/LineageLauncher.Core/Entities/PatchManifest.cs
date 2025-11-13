namespace LineageLauncher.Core.Entities;

/// <summary>
/// Represents a collection of files that need to be patched.
/// </summary>
public sealed class PatchManifest
{
    public required string Version { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IReadOnlyList<PatchFile> Files { get; init; }
    public long TotalSize => Files.Sum(f => f.CompressedSize);
    public int FileCount => Files.Count;
}

/// <summary>
/// Represents a single file in a patch manifest.
/// </summary>
public sealed class PatchFile
{
    public required string RelativePath { get; init; }
    public required string Checksum { get; init; }
    public required long CompressedSize { get; init; }
    public required long UncompressedSize { get; init; }
    public required string DownloadUrl { get; init; }
}
