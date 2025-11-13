using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LineageLauncher.Launcher.Native;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher.Injection;

/// <summary>
/// Injects DLLs into a suspended process using LoadLibraryW.
/// </summary>
public sealed class DllInjector : IDisposable
{
    private readonly ILogger<DllInjector> _logger;
    private readonly List<IntPtr> _allocatedMemory = new();
    private bool _disposed;

    private const int INJECTION_TIMEOUT_MS = 5000;

    public DllInjector(ILogger<DllInjector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Injects multiple DLLs sequentially into the target process.
    /// </summary>
    public async Task<DllInjectionResult> InjectDllsAsync(
        IntPtr processHandle,
        IEnumerable<string> dllPaths,
        CancellationToken cancellationToken = default)
    {
        if (processHandle == IntPtr.Zero)
        {
            return DllInjectionResult.CreateFailed(
                "Invalid process handle",
                null);
        }

        var dllList = dllPaths.ToList();
        if (dllList.Count == 0)
        {
            return DllInjectionResult.CreateFailed(
                "No DLLs specified for injection",
                null);
        }

        _logger.LogInformation(
            "Starting injection of {Count} DLLs",
            dllList.Count);

        var injectedDlls = new List<string>();

        foreach (var dllPath in dllList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Injecting: {DllPath}", dllPath);

            var result = await InjectSingleDllAsync(
                processHandle,
                dllPath,
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogError(
                    "Failed to inject {DllPath}: {Error}",
                    dllPath,
                    result.ErrorMessage);

                return DllInjectionResult.CreateFailed(
                    result.ErrorMessage,
                    dllPath,
                    injectedDlls);
            }

            injectedDlls.Add(dllPath);
            _logger.LogInformation(
                "Successfully injected: {DllPath}",
                Path.GetFileName(dllPath));
        }

        _logger.LogInformation(
            "All {Count} DLLs injected successfully",
            injectedDlls.Count);

        return DllInjectionResult.CreateSuccess(injectedDlls);
    }

    /// <summary>
    /// Injects a single DLL into the target process.
    /// </summary>
    public async Task<DllInjectionResult> InjectSingleDllAsync(
        IntPtr processHandle,
        string dllPath,
        CancellationToken cancellationToken = default)
    {
        // Validate DLL path
        if (!File.Exists(dllPath))
        {
            return DllInjectionResult.CreateFailed(
                $"DLL not found: {dllPath}",
                dllPath);
        }

        // Get full path (required for LoadLibraryW)
        var fullDllPath = Path.GetFullPath(dllPath);
        _logger.LogDebug("Full DLL path: {FullPath}", fullDllPath);

        // Convert path to Unicode bytes (LoadLibraryW uses Unicode)
        var dllPathBytes = Encoding.Unicode.GetBytes(fullDllPath + "\0");

        IntPtr remoteMemory = IntPtr.Zero;
        IntPtr threadHandle = IntPtr.Zero;

        try
        {
            // Step 1: Allocate memory in remote process
            _logger.LogDebug(
                "Allocating {Size} bytes in remote process",
                dllPathBytes.Length);

            remoteMemory = NativeInterop.AllocateMemory(
                processHandle,
                (uint)dllPathBytes.Length,
                AllocationType.Commit | AllocationType.Reserve,
                MemoryProtection.ReadWrite);

            if (remoteMemory == IntPtr.Zero)
            {
                var errorMsg = NativeInterop.GetLastErrorMessage();
                return DllInjectionResult.CreateFailed(
                    $"VirtualAllocEx failed: {errorMsg}",
                    dllPath);
            }

            _logger.LogDebug(
                "Memory allocated at: {Address:X}",
                remoteMemory.ToInt64());

            // Step 2: Write DLL path to remote memory
            var writeSuccess = NativeInterop.WriteMemory(
                processHandle,
                remoteMemory,
                dllPathBytes,
                out var bytesWritten);

            if (!writeSuccess || bytesWritten != dllPathBytes.Length)
            {
                var errorMsg = NativeInterop.GetLastErrorMessage();
                return DllInjectionResult.CreateFailed(
                    $"WriteProcessMemory failed: {errorMsg}",
                    dllPath);
            }

            _logger.LogDebug("Written {BytesWritten} bytes", bytesWritten);

            // Step 3: Get LoadLibraryW address from kernel32.dll
            var kernel32Handle = NativeInterop.GetModuleHandle("kernel32.dll");
            if (kernel32Handle == IntPtr.Zero)
            {
                return DllInjectionResult.CreateFailed(
                    "Failed to get kernel32.dll handle",
                    dllPath);
            }

            var loadLibraryAddr = NativeInterop.GetFunctionAddress(
                kernel32Handle,
                "LoadLibraryW");

            if (loadLibraryAddr == IntPtr.Zero)
            {
                return DllInjectionResult.CreateFailed(
                    "Failed to get LoadLibraryW address",
                    dllPath);
            }

            _logger.LogDebug(
                "LoadLibraryW address: {Address:X}",
                loadLibraryAddr.ToInt64());

            // Step 4: Create remote thread to call LoadLibraryW
            threadHandle = NativeInterop.CreateThread(
                processHandle,
                loadLibraryAddr,
                remoteMemory,
                out var threadId);

            if (threadHandle == IntPtr.Zero)
            {
                var errorMsg = NativeInterop.GetLastErrorMessage();
                return DllInjectionResult.CreateFailed(
                    $"CreateRemoteThread failed: {errorMsg}",
                    dllPath);
            }

            _logger.LogDebug("Remote thread created: {ThreadId}", threadId);

            // Step 5: Wait for thread completion
            var waitResult = await Task.Run(() =>
                NativeInterop.WaitForSingleObject(
                    threadHandle,
                    INJECTION_TIMEOUT_MS),
                cancellationToken);

            if (waitResult == NativeInterop.WAIT_TIMEOUT)
            {
                return DllInjectionResult.CreateFailed(
                    $"Injection timed out after {INJECTION_TIMEOUT_MS}ms",
                    dllPath);
            }

            if (waitResult != NativeInterop.WAIT_OBJECT_0)
            {
                return DllInjectionResult.CreateFailed(
                    $"Thread wait failed with result: {waitResult}",
                    dllPath);
            }

            _logger.LogDebug("Remote thread completed successfully");
            return DllInjectionResult.CreateSuccess(new List<string> { dllPath });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Injection cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during injection");
            return DllInjectionResult.CreateFailed(
                $"Unexpected error: {ex.Message}",
                dllPath);
        }
        finally
        {
            // Cleanup: Free remote memory
            if (remoteMemory != IntPtr.Zero)
            {
                NativeInterop.FreeMemory(
                    processHandle,
                    remoteMemory,
                    0,
                    FreeType.Release);
            }

            // Close thread handle
            if (threadHandle != IntPtr.Zero)
            {
                NativeInterop.CloseHandle(threadHandle);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _allocatedMemory.Clear();
        _disposed = true;

        _logger.LogDebug("DllInjector disposed");
    }
}

/// <summary>
/// Result of DLL injection operation.
/// </summary>
public sealed class DllInjectionResult
{
    public bool Success { get; init; }
    public int InjectedCount { get; init; }
    public List<string> InjectedDlls { get; init; } = new();
    public string? ErrorMessage { get; init; }
    public string? FailedDll { get; init; }

    public static DllInjectionResult CreateSuccess(List<string> injectedDlls)
    {
        return new DllInjectionResult
        {
            Success = true,
            InjectedCount = injectedDlls.Count,
            InjectedDlls = injectedDlls
        };
    }

    public static DllInjectionResult CreateFailed(
        string? errorMessage,
        string? failedDll,
        List<string>? partiallyInjected = null)
    {
        return new DllInjectionResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            FailedDll = failedDll,
            InjectedDlls = partiallyInjected ?? new List<string>(),
            InjectedCount = partiallyInjected?.Count ?? 0
        };
    }
}
