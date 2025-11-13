using System;
using System.Collections.Generic;
using System.IO;
using LineageLauncher.Launcher.Native;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher.Process;

/// <summary>
/// Creates and manages the lifecycle of a suspended process.
/// </summary>
public sealed class ProcessCreator : IDisposable
{
    private readonly ILogger<ProcessCreator> _logger;
    private IntPtr _processHandle;
    private IntPtr _threadHandle;
    private int _processId;
    private bool _disposed;

    public bool IsRunning => _processHandle != IntPtr.Zero;
    public int ProcessId => _processId;
    public IntPtr ProcessHandle => _processHandle;
    public IntPtr MainThreadHandle => _threadHandle;

    public ProcessCreator(ILogger<ProcessCreator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a process in suspended state.
    /// </summary>
    public ProcessCreationResult CreateSuspended(
        string executablePath,
        string commandLine,
        string workingDirectory,
        IDictionary<string, string>? environmentVariables = null)
    {
        _logger.LogInformation(
            "Creating suspended process: {Path}",
            executablePath);

        // Validation
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return ProcessCreationResult.CreateFailed(
                "Executable path is required");
        }

        if (!File.Exists(executablePath))
        {
            return ProcessCreationResult.CreateFailed(
                $"Executable not found: {executablePath}");
        }

        if (!Directory.Exists(workingDirectory))
        {
            return ProcessCreationResult.CreateFailed(
                $"Working directory not found: {workingDirectory}");
        }

        try
        {
            // Create process with CREATE_SUSPENDED flag
            var success = NativeInterop.CreateProcess(
                executablePath,
                commandLine ?? string.Empty,
                inheritHandles: false,
                ProcessCreationFlags.CreateSuspended | ProcessCreationFlags.CreateNewConsole,
                workingDirectory,
                out var processInfo);

            if (!success)
            {
                var errorMessage = NativeInterop.GetLastErrorMessage();
                _logger.LogError("CreateProcess failed: {Error}", errorMessage);
                return ProcessCreationResult.CreateFailed(
                    $"Failed to create process: {errorMessage}");
            }

            // Store process information
            _processHandle = processInfo.ProcessHandle;
            _threadHandle = processInfo.ThreadHandle;
            _processId = processInfo.ProcessId;

            _logger.LogInformation(
                "Process created successfully. PID: {ProcessId}, Handle: {Handle:X}",
                _processId,
                _processHandle);

            return ProcessCreationResult.CreateSuccess(
                _processId,
                _processHandle,
                _threadHandle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating process");
            return ProcessCreationResult.CreateFailed(
                $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Resumes the main thread of the suspended process.
    /// </summary>
    public void ResumeMainThread()
    {
        if (_threadHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "No thread handle available. Process may not be created or already resumed.");
        }

        _logger.LogInformation("Resuming main thread...");

        var suspendCount = NativeInterop.ResumeThread(_threadHandle);

        if (suspendCount == uint.MaxValue)
        {
            var errorMessage = NativeInterop.GetLastErrorMessage();
            _logger.LogError("ResumeThread failed: {Error}", errorMessage);
            throw new InvalidOperationException(
                $"Failed to resume thread: {errorMessage}");
        }

        _logger.LogInformation(
            "Thread resumed. Previous suspend count: {SuspendCount}",
            suspendCount);
    }

    /// <summary>
    /// Suspends the main thread of the process.
    /// </summary>
    public void SuspendMainThread()
    {
        if (_threadHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "No thread handle available.");
        }

        _logger.LogInformation("Suspending main thread...");

        var suspendCount = NativeInterop.SuspendThread(_threadHandle);

        if (suspendCount == uint.MaxValue)
        {
            var errorMessage = NativeInterop.GetLastErrorMessage();
            _logger.LogError("SuspendThread failed: {Error}", errorMessage);
            throw new InvalidOperationException(
                $"Failed to suspend thread: {errorMessage}");
        }

        _logger.LogInformation(
            "Thread suspended. New suspend count: {SuspendCount}",
            suspendCount);
    }

    /// <summary>
    /// Terminates the process if it is running.
    /// </summary>
    public void Terminate()
    {
        if (_processHandle == IntPtr.Zero)
        {
            _logger.LogWarning("No process to terminate");
            return;
        }

        _logger.LogInformation("Terminating process: {ProcessId}", _processId);

        var success = NativeInterop.Terminate(_processHandle, 1);

        if (!success)
        {
            var errorMessage = NativeInterop.GetLastErrorMessage();
            _logger.LogError(
                "Failed to terminate process: {Error}",
                errorMessage);
        }
        else
        {
            _logger.LogInformation("Process terminated successfully");
        }

        Cleanup();
    }

    private void Cleanup()
    {
        if (_threadHandle != IntPtr.Zero)
        {
            NativeInterop.CloseHandle(_threadHandle);
            _threadHandle = IntPtr.Zero;
        }

        if (_processHandle != IntPtr.Zero)
        {
            NativeInterop.CloseHandle(_processHandle);
            _processHandle = IntPtr.Zero;
        }

        _processId = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;

        Cleanup();
        _disposed = true;

        _logger.LogDebug("ProcessCreator disposed");
    }
}

/// <summary>
/// Result of process creation operation.
/// </summary>
public sealed class ProcessCreationResult
{
    public bool Success { get; init; }
    public int ProcessId { get; init; }
    public IntPtr ProcessHandle { get; init; }
    public IntPtr ThreadHandle { get; init; }
    public string? ErrorMessage { get; init; }

    public static ProcessCreationResult CreateSuccess(
        int processId,
        IntPtr processHandle,
        IntPtr threadHandle)
    {
        return new ProcessCreationResult
        {
            Success = true,
            ProcessId = processId,
            ProcessHandle = processHandle,
            ThreadHandle = threadHandle
        };
    }

    public static ProcessCreationResult CreateFailed(string errorMessage)
    {
        return new ProcessCreationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
