# DLL Injection Class Templates
## Ready-to-Implement Code Structure

**Date**: 2025-11-12
**Purpose**: Provide complete class templates for implementation

---

## Table of Contents

1. [NativeInterop.cs](#nativeinteropcs)
2. [NativeStructures.cs](#nativestructurescs)
3. [ProcessCreator.cs](#processcreatorcs)
4. [DllInjector.cs](#dllinjectorcs)
5. [PipeManager.cs](#pipemanagercs)
6. [ProcessLaunchOrchestrator.cs](#processlaunchorchestratorcs)
7. [Result Classes](#result-classes)
8. [Configuration Classes](#configuration-classes)

---

## NativeInterop.cs

**Location**: `src/LineageLauncher.Launcher/Native/NativeInterop.cs`

```csharp
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace LineageLauncher.Launcher.Native;

/// <summary>
/// P/Invoke declarations for Windows API functions required for DLL injection.
/// </summary>
public static class NativeInterop
{
    private const string KERNEL32 = "kernel32.dll";

    #region Process Creation

    [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessW(
        string? lpApplicationName,
        StringBuilder? lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        ProcessCreationFlags dwCreationFlags,
        IntPtr lpEnvironment,
        string? lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    /// <summary>
    /// Creates a new process with specified flags.
    /// </summary>
    public static bool CreateProcess(
        string applicationName,
        string commandLine,
        bool inheritHandles,
        ProcessCreationFlags creationFlags,
        string currentDirectory,
        out PROCESS_INFORMATION processInfo)
    {
        var startupInfo = new STARTUPINFO
        {
            cb = Marshal.SizeOf<STARTUPINFO>()
        };

        var cmdLine = string.IsNullOrEmpty(commandLine)
            ? null
            : new StringBuilder(commandLine);

        var result = CreateProcessW(
            applicationName,
            cmdLine,
            IntPtr.Zero,
            IntPtr.Zero,
            inheritHandles,
            creationFlags,
            IntPtr.Zero,
            currentDirectory,
            ref startupInfo,
            out processInfo);

        return result;
    }

    #endregion

    #region Memory Management

    [DllImport(KERNEL32, SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        uint dwSize,
        AllocationType flAllocationType,
        MemoryProtection flProtect);

    /// <summary>
    /// Allocates memory in the virtual address space of the specified process.
    /// </summary>
    public static IntPtr AllocateMemory(
        IntPtr processHandle,
        uint size,
        AllocationType allocationType,
        MemoryProtection protection)
    {
        return VirtualAllocEx(
            processHandle,
            IntPtr.Zero,
            size,
            allocationType,
            protection);
    }

    [DllImport(KERNEL32, SetLastError = true)]
    private static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        uint nSize,
        out IntPtr lpNumberOfBytesWritten);

    /// <summary>
    /// Writes data to an area of memory in a specified process.
    /// </summary>
    public static bool WriteMemory(
        IntPtr processHandle,
        IntPtr baseAddress,
        byte[] buffer,
        out int bytesWritten)
    {
        var result = WriteProcessMemory(
            processHandle,
            baseAddress,
            buffer,
            (uint)buffer.Length,
            out var written);

        bytesWritten = written.ToInt32();
        return result;
    }

    [DllImport(KERNEL32, SetLastError = true)]
    private static extern bool VirtualFreeEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        uint dwSize,
        FreeType dwFreeType);

    /// <summary>
    /// Releases or decommits memory within the virtual address space of a process.
    /// </summary>
    public static bool FreeMemory(
        IntPtr processHandle,
        IntPtr address,
        uint size,
        FreeType freeType)
    {
        return VirtualFreeEx(processHandle, address, size, freeType);
    }

    #endregion

    #region Thread Management

    [DllImport(KERNEL32, SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        out IntPtr lpThreadId);

    /// <summary>
    /// Creates a thread that runs in the virtual address space of another process.
    /// </summary>
    public static IntPtr CreateThread(
        IntPtr processHandle,
        IntPtr startAddress,
        IntPtr parameter,
        out IntPtr threadId)
    {
        return CreateRemoteThread(
            processHandle,
            IntPtr.Zero,
            0,
            startAddress,
            parameter,
            0,
            out threadId);
    }

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint ResumeThread(IntPtr hThread);

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint SuspendThread(IntPtr hThread);

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern uint WaitForSingleObject(
        IntPtr hHandle,
        uint dwMilliseconds);

    #endregion

    #region Module Information

    [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandleW(string? lpModuleName);

    /// <summary>
    /// Retrieves a module handle for the specified module.
    /// </summary>
    public static IntPtr GetModuleHandle(string moduleName)
    {
        return GetModuleHandleW(moduleName);
    }

    [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string lpProcName);

    /// <summary>
    /// Retrieves the address of an exported function from a DLL.
    /// </summary>
    public static IntPtr GetFunctionAddress(IntPtr moduleHandle, string functionName)
    {
        return GetProcAddress(moduleHandle, functionName);
    }

    #endregion

    #region Process Management

    [DllImport(KERNEL32, SetLastError = true)]
    private static extern bool TerminateProcess(
        IntPtr hProcess,
        uint uExitCode);

    /// <summary>
    /// Terminates the specified process.
    /// </summary>
    public static bool Terminate(IntPtr processHandle, uint exitCode = 0)
    {
        return TerminateProcess(processHandle, exitCode);
    }

    [DllImport(KERNEL32, SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    #endregion

    #region Error Handling

    [DllImport(KERNEL32)]
    private static extern int GetLastError();

    /// <summary>
    /// Retrieves the last Win32 error code.
    /// </summary>
    public static int GetLastWin32Error()
    {
        return Marshal.GetLastWin32Error();
    }

    /// <summary>
    /// Gets a user-friendly error message for the last Win32 error.
    /// </summary>
    public static string GetLastErrorMessage()
    {
        var errorCode = Marshal.GetLastWin32Error();
        return new Win32Exception(errorCode).Message;
    }

    #endregion

    #region Constants

    public const uint INFINITE = 0xFFFFFFFF;
    public const uint WAIT_OBJECT_0 = 0x00000000;
    public const uint WAIT_TIMEOUT = 0x00000102;
    public const uint WAIT_FAILED = 0xFFFFFFFF;

    #endregion
}
```

---

## NativeStructures.cs

**Location**: `src/LineageLauncher.Launcher/Native/NativeStructures.cs`

```csharp
using System;
using System.Runtime.InteropServices;

namespace LineageLauncher.Launcher.Native;

/// <summary>
/// Native Windows structures for process creation and management.
/// </summary>

#region Structures

[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_INFORMATION
{
    public IntPtr ProcessHandle;
    public IntPtr ThreadHandle;
    public int ProcessId;
    public int ThreadId;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct STARTUPINFO
{
    public int cb;
    public string? lpReserved;
    public string? lpDesktop;
    public string? lpTitle;
    public int dwX;
    public int dwY;
    public int dwXSize;
    public int dwYSize;
    public int dwXCountChars;
    public int dwYCountChars;
    public int dwFillAttribute;
    public int dwFlags;
    public short wShowWindow;
    public short cbReserved2;
    public IntPtr lpReserved2;
    public IntPtr hStdInput;
    public IntPtr hStdOutput;
    public IntPtr hStdError;
}

#endregion

#region Enums

[Flags]
public enum ProcessCreationFlags : uint
{
    None = 0x00000000,
    DebugProcess = 0x00000001,
    DebugOnlyThisProcess = 0x00000002,
    CreateSuspended = 0x00000004,
    DetachedProcess = 0x00000008,
    CreateNewConsole = 0x00000010,
    NormalPriorityClass = 0x00000020,
    IdlePriorityClass = 0x00000040,
    HighPriorityClass = 0x00000080,
    RealtimePriorityClass = 0x00000100,
    CreateNewProcessGroup = 0x00000200,
    CreateUnicodeEnvironment = 0x00000400,
    CreateSeparateWowVdm = 0x00000800,
    CreateSharedWowVdm = 0x00001000,
    CreateForcedos = 0x00002000,
    BelowNormalPriorityClass = 0x00004000,
    AboveNormalPriorityClass = 0x00008000,
    InheritParentAffinity = 0x00010000,
    InheritCallerPriority = 0x00020000,
    CreateProtectedProcess = 0x00040000,
    ExtendedStartupinfoPresent = 0x00080000,
    ProcessModeBackgroundBegin = 0x00100000,
    ProcessModeBackgroundEnd = 0x00200000,
    CreateBreakawayFromJob = 0x01000000,
    CreatePreserveCodeAuthzLevel = 0x02000000,
    CreateDefaultErrorMode = 0x04000000,
    CreateNoWindow = 0x08000000,
    ProfileUser = 0x10000000,
    ProfileKernel = 0x20000000,
    ProfileServer = 0x40000000,
    CreateIgnoreSystemDefault = 0x80000000,
}

[Flags]
public enum ProcessAccessFlags : uint
{
    All = 0x001F0FFF,
    Terminate = 0x00000001,
    CreateThread = 0x00000002,
    VirtualMemoryOperation = 0x00000008,
    VirtualMemoryRead = 0x00000010,
    VirtualMemoryWrite = 0x00000020,
    DuplicateHandle = 0x00000040,
    CreateProcess = 0x00000080,
    SetQuota = 0x00000100,
    SetInformation = 0x00000200,
    QueryInformation = 0x00000400,
    QueryLimitedInformation = 0x00001000,
    Synchronize = 0x00100000
}

[Flags]
public enum AllocationType : uint
{
    Commit = 0x00001000,
    Reserve = 0x00002000,
    Decommit = 0x00004000,
    Release = 0x00008000,
    Reset = 0x00080000,
    Physical = 0x00400000,
    TopDown = 0x00100000,
    WriteWatch = 0x00200000,
    LargePages = 0x20000000
}

[Flags]
public enum MemoryProtection : uint
{
    NoAccess = 0x01,
    ReadOnly = 0x02,
    ReadWrite = 0x04,
    WriteCopy = 0x08,
    Execute = 0x10,
    ExecuteRead = 0x20,
    ExecuteReadWrite = 0x40,
    ExecuteWriteCopy = 0x80,
    GuardModifierflag = 0x100,
    NoCacheModifierflag = 0x200,
    WriteCombineModifierflag = 0x400
}

[Flags]
public enum FreeType : uint
{
    Decommit = 0x00004000,
    Release = 0x00008000,
}

#endregion
```

---

## ProcessCreator.cs

**Location**: `src/LineageLauncher.Launcher/Process/ProcessCreator.cs`

```csharp
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
            return ProcessCreationResult.Failed(
                "Executable path is required");
        }

        if (!File.Exists(executablePath))
        {
            return ProcessCreationResult.Failed(
                $"Executable not found: {executablePath}");
        }

        if (!Directory.Exists(workingDirectory))
        {
            return ProcessCreationResult.Failed(
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
                return ProcessCreationResult.Failed(
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

            return ProcessCreationResult.Success(
                _processId,
                _processHandle,
                _threadHandle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating process");
            return ProcessCreationResult.Failed(
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

    public static ProcessCreationResult Success(
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

    public static ProcessCreationResult Failed(string errorMessage)
    {
        return new ProcessCreationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
```

---

## DllInjector.cs

**Location**: `src/LineageLauncher.Launcher/Injection/DllInjector.cs`

```csharp
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
            return DllInjectionResult.Failed(
                "Invalid process handle",
                null);
        }

        var dllList = dllPaths.ToList();
        if (dllList.Count == 0)
        {
            return DllInjectionResult.Failed(
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

                return DllInjectionResult.Failed(
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

        return DllInjectionResult.Success(injectedDlls);
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
            return DllInjectionResult.Failed(
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
                return DllInjectionResult.Failed(
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
                return DllInjectionResult.Failed(
                    $"WriteProcessMemory failed: {errorMsg}",
                    dllPath);
            }

            _logger.LogDebug("Written {BytesWritten} bytes", bytesWritten);

            // Step 3: Get LoadLibraryW address from kernel32.dll
            var kernel32Handle = NativeInterop.GetModuleHandle("kernel32.dll");
            if (kernel32Handle == IntPtr.Zero)
            {
                return DllInjectionResult.Failed(
                    "Failed to get kernel32.dll handle",
                    dllPath);
            }

            var loadLibraryAddr = NativeInterop.GetFunctionAddress(
                kernel32Handle,
                "LoadLibraryW");

            if (loadLibraryAddr == IntPtr.Zero)
            {
                return DllInjectionResult.Failed(
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
                return DllInjectionResult.Failed(
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
                return DllInjectionResult.Failed(
                    $"Injection timed out after {INJECTION_TIMEOUT_MS}ms",
                    dllPath);
            }

            if (waitResult != NativeInterop.WAIT_OBJECT_0)
            {
                return DllInjectionResult.Failed(
                    $"Thread wait failed with result: {waitResult}",
                    dllPath);
            }

            _logger.LogDebug("Remote thread completed successfully");
            return DllInjectionResult.Success(new List<string> { dllPath });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Injection cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during injection");
            return DllInjectionResult.Failed(
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

    public static DllInjectionResult Success(List<string> injectedDlls)
    {
        return new DllInjectionResult
        {
            Success = true,
            InjectedCount = injectedDlls.Count,
            InjectedDlls = injectedDlls
        };
    }

    public static DllInjectionResult Failed(
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
```

---

## PipeManager.cs

**Location**: `src/LineageLauncher.Launcher/IPC/PipeManager.cs`

```csharp
using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher.IPC;

/// <summary>
/// Manages named pipes for bidirectional communication with the game client.
/// </summary>
public sealed class PipeManager : IDisposable
{
    private readonly ILogger<PipeManager> _logger;
    private readonly string _pipeNameOut;
    private readonly string _pipeNameIn;

    private NamedPipeServerStream? _pipeOut;
    private NamedPipeServerStream? _pipeIn;
    private bool _disposed;

    public bool IsConnected =>
        _pipeOut?.IsConnected == true && _pipeIn?.IsConnected == true;

    public PipeManager(
        ILogger<PipeManager> logger,
        string? pipeBaseName = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var baseName = pipeBaseName ?? "LineageLauncher";
        _pipeNameOut = $"{baseName}_Pipe1";
        _pipeNameIn = $"{baseName}_Pipe2";
    }

    /// <summary>
    /// Creates the named pipes for IPC.
    /// </summary>
    public Task CreatePipesAsync(CancellationToken cancellationToken = default)
    {
        if (_pipeOut != null || _pipeIn != null)
        {
            throw new InvalidOperationException("Pipes already created");
        }

        try
        {
            _logger.LogInformation("Creating named pipes...");
            _logger.LogDebug("Pipe Out (Launcher → Game): {PipeName}", _pipeNameOut);
            _logger.LogDebug("Pipe In (Game → Launcher): {PipeName}", _pipeNameIn);

            // Pipe 1: Launcher → Game (Out)
            _pipeOut = new NamedPipeServerStream(
                _pipeNameOut,
                PipeDirection.Out,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            // Pipe 2: Game → Launcher (In)
            _pipeIn = new NamedPipeServerStream(
                _pipeNameIn,
                PipeDirection.In,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            _logger.LogInformation("Named pipes created successfully");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create named pipes");
            Dispose();
            throw;
        }
    }

    /// <summary>
    /// Waits for the game client to connect to both pipes.
    /// </summary>
    public async Task WaitForConnectionAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (_pipeOut == null || _pipeIn == null)
        {
            throw new InvalidOperationException("Pipes not created");
        }

        _logger.LogInformation(
            "Waiting for game client connection (timeout: {Timeout})...",
            timeout);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            // Wait for both pipes to connect
            await Task.WhenAll(
                _pipeOut.WaitForConnectionAsync(cts.Token),
                _pipeIn.WaitForConnectionAsync(cts.Token));

            _logger.LogInformation("Game client connected to both pipes");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Pipe connection timeout after {Timeout}",
                timeout);
            throw new TimeoutException(
                $"Game client did not connect within {timeout}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Pipe connection cancelled");
            throw;
        }
    }

    /// <summary>
    /// Reads a message from the game client.
    /// </summary>
    public async Task<byte[]?> ReadMessageAsync(
        CancellationToken cancellationToken = default)
    {
        if (_pipeIn == null)
        {
            throw new InvalidOperationException("Pipe not created");
        }

        if (!_pipeIn.IsConnected)
        {
            _logger.LogWarning("Pipe not connected");
            return null;
        }

        try
        {
            var buffer = new byte[4096];
            var bytesRead = await _pipeIn.ReadAsync(
                buffer,
                0,
                buffer.Length,
                cancellationToken);

            if (bytesRead == 0)
            {
                _logger.LogDebug("Client disconnected (0 bytes read)");
                return null;
            }

            var message = new byte[bytesRead];
            Array.Copy(buffer, message, bytesRead);

            _logger.LogDebug("Received {BytesRead} bytes", bytesRead);
            return message;
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Pipe read error");
            return null;
        }
    }

    /// <summary>
    /// Writes a message to the game client.
    /// </summary>
    public async Task WriteMessageAsync(
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        if (_pipeOut == null)
        {
            throw new InvalidOperationException("Pipe not created");
        }

        if (!_pipeOut.IsConnected)
        {
            throw new InvalidOperationException("Pipe not connected");
        }

        try
        {
            await _pipeOut.WriteAsync(data, 0, data.Length, cancellationToken);
            await _pipeOut.FlushAsync(cancellationToken);

            _logger.LogDebug("Sent {BytesSent} bytes", data.Length);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Pipe write error");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _pipeOut?.Dispose();
        _pipeIn?.Dispose();

        _pipeOut = null;
        _pipeIn = null;

        _disposed = true;
        _logger.LogDebug("PipeManager disposed");
    }
}
```

---

## ProcessLaunchOrchestrator.cs

**Location**: `src/LineageLauncher.Launcher/Orchestration/ProcessLaunchOrchestrator.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using LineageLauncher.Launcher.Process;
using LineageLauncher.Launcher.Injection;
using LineageLauncher.Launcher.IPC;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher.Orchestration;

/// <summary>
/// Orchestrates the complete process launch with DLL injection and IPC setup.
/// </summary>
public sealed class ProcessLaunchOrchestrator : IDisposable
{
    private readonly ILogger<ProcessLaunchOrchestrator> _logger;
    private readonly ProcessCreator _processCreator;
    private readonly DllInjector _dllInjector;
    private readonly PipeManager _pipeManager;
    private bool _disposed;

    public bool IsProcessRunning => _processCreator.IsRunning;
    public int ProcessId => _processCreator.ProcessId;

    public ProcessLaunchOrchestrator(
        ILogger<ProcessLaunchOrchestrator> logger,
        ProcessCreator processCreator,
        DllInjector dllInjector,
        PipeManager pipeManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processCreator = processCreator ?? throw new ArgumentNullException(nameof(processCreator));
        _dllInjector = dllInjector ?? throw new ArgumentNullException(nameof(dllInjector));
        _pipeManager = pipeManager ?? throw new ArgumentNullException(nameof(pipeManager));
    }

    /// <summary>
    /// Launches the game with DLL injection following the complete flow.
    /// </summary>
    public async Task<LaunchResult> LaunchWithInjectionAsync(
        LaunchConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== Starting Launch with Injection ===");
        _logger.LogInformation("Executable: {Path}", config.ExecutablePath);
        _logger.LogInformation("DLLs to inject: {Count}", config.DllsToInject.Count);

        var currentPhase = LaunchPhase.None;

        try
        {
            // Phase 1: Create Named Pipes
            currentPhase = LaunchPhase.PipeCreation;
            progress?.Report("Creating IPC pipes...");
            _logger.LogInformation("Phase 1: Creating named pipes");

            await _pipeManager.CreatePipesAsync(cancellationToken);

            _logger.LogInformation("✓ Phase 1 complete");

            // Phase 2: Create Process (Suspended)
            currentPhase = LaunchPhase.ProcessCreation;
            progress?.Report("Creating game process...");
            _logger.LogInformation("Phase 2: Creating suspended process");

            var createResult = _processCreator.CreateSuspended(
                config.ExecutablePath,
                config.CommandLine ?? string.Empty,
                config.WorkingDirectory,
                config.EnvironmentVariables);

            if (!createResult.Success)
            {
                _logger.LogError(
                    "Process creation failed: {Error}",
                    createResult.ErrorMessage);

                return LaunchResult.Failed(
                    currentPhase,
                    createResult.ErrorMessage);
            }

            _logger.LogInformation(
                "✓ Phase 2 complete (PID: {ProcessId})",
                createResult.ProcessId);

            // Phase 3: Inject DLLs
            currentPhase = LaunchPhase.DllInjection;
            progress?.Report($"Injecting {config.DllsToInject.Count} DLLs...");
            _logger.LogInformation("Phase 3: Injecting DLLs");

            var injectionResult = await _dllInjector.InjectDllsAsync(
                createResult.ProcessHandle,
                config.DllsToInject,
                cancellationToken);

            if (!injectionResult.Success)
            {
                _logger.LogError(
                    "DLL injection failed: {Error}",
                    injectionResult.ErrorMessage);

                // Rollback: Terminate process
                _logger.LogWarning("Rolling back: Terminating process");
                _processCreator.Terminate();

                return LaunchResult.Failed(
                    currentPhase,
                    $"DLL injection failed: {injectionResult.ErrorMessage}");
            }

            _logger.LogInformation(
                "✓ Phase 3 complete ({Count} DLLs injected)",
                injectionResult.InjectedCount);

            // Phase 4: Resume Main Thread
            currentPhase = LaunchPhase.ThreadResume;
            progress?.Report("Starting game...");
            _logger.LogInformation("Phase 4: Resuming main thread");

            _processCreator.ResumeMainThread();

            _logger.LogInformation("✓ Phase 4 complete");

            // Phase 5: Wait for Pipe Connection (Optional)
            currentPhase = LaunchPhase.PipeConnection;
            progress?.Report("Waiting for game initialization...");
            _logger.LogInformation("Phase 5: Waiting for pipe connection");

            try
            {
                await _pipeManager.WaitForConnectionAsync(
                    config.PipeConnectionTimeout,
                    cancellationToken);

                _logger.LogInformation("✓ Phase 5 complete (Pipe connected)");
            }
            catch (TimeoutException)
            {
                _logger.LogWarning(
                    "Pipe connection timeout (this may be normal behavior)");
            }

            // Success
            progress?.Report("Game launched successfully!");
            _logger.LogInformation("=== Launch Complete ===");

            return LaunchResult.Success(createResult.ProcessId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Launch cancelled by user");
            await TerminateAsync();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during launch");
            await TerminateAsync();

            return LaunchResult.Failed(
                currentPhase,
                $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Terminates the running game process.
    /// </summary>
    public Task TerminateAsync()
    {
        _logger.LogInformation("Terminating game process...");
        _processCreator.Terminate();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _processCreator.Dispose();
        _dllInjector.Dispose();
        _pipeManager.Dispose();

        _disposed = true;
        _logger.LogDebug("ProcessLaunchOrchestrator disposed");
    }
}

/// <summary>
/// Configuration for launching a process with DLL injection.
/// </summary>
public sealed class LaunchConfiguration
{
    public required string ExecutablePath { get; init; }
    public required string WorkingDirectory { get; init; }
    public required List<string> DllsToInject { get; init; }
    public string? CommandLine { get; init; }
    public Dictionary<string, string>? EnvironmentVariables { get; init; }
    public TimeSpan PipeConnectionTimeout { get; init; } = TimeSpan.FromSeconds(10);
}

/// <summary>
/// Result of the complete launch operation.
/// </summary>
public sealed class LaunchResult
{
    public bool Success { get; init; }
    public int ProcessId { get; init; }
    public string? ErrorMessage { get; init; }
    public LaunchPhase FailedPhase { get; init; }

    public static LaunchResult Success(int processId)
    {
        return new LaunchResult
        {
            Success = true,
            ProcessId = processId
        };
    }

    public static LaunchResult Failed(LaunchPhase phase, string? errorMessage)
    {
        return new LaunchResult
        {
            Success = false,
            FailedPhase = phase,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Phases of the launch process.
/// </summary>
public enum LaunchPhase
{
    None,
    PipeCreation,
    ProcessCreation,
    DllInjection,
    ThreadResume,
    PipeConnection
}
```

---

## Summary

This document provides complete, ready-to-implement class templates for the DLL injection architecture. Each class:

- Follows modern C# patterns (async/await, IDisposable, nullable reference types)
- Includes comprehensive error handling
- Provides detailed logging
- Is fully documented with XML comments
- Implements dependency injection
- Uses proper resource management

**Total Lines of Code**: ~2,000 lines across 7 files

**Next Step**: Begin implementation starting with Phase 1 (NativeInterop and NativeStructures).
