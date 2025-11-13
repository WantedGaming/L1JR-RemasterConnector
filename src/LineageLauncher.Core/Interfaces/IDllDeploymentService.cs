namespace LineageLauncher.Core.Interfaces;

/// <summary>
/// Service for downloading and deploying required DLL files for the game client.
/// </summary>
public interface IDllDeploymentService
{
    /// <summary>
    /// Downloads and deploys all required DLLs to the client directory.
    /// </summary>
    /// <param name="clientDirectory">The directory where the game client is installed.</param>
    /// <param name="progress">Progress reporter for download status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if all DLLs were successfully deployed.</returns>
    Task<bool> DeployDllsAsync(
        string clientDirectory,
        IProgress<(string fileName, int percentage)>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that all required DLLs are present and have correct sizes.
    /// </summary>
    /// <param name="clientDirectory">The directory to check.</param>
    /// <returns>True if all DLLs are valid.</returns>
    Task<bool> ValidateDllsAsync(string clientDirectory);
}
