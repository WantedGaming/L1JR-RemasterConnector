# DLL Injection Architecture Design
## Lineage Remaster Custom Launcher

**Date**: 2025-11-12
**Version**: 1.0
**Author**: Backend Architect
**Target Framework**: .NET 8.0

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Architecture Overview](#architecture-overview)
3. [Component Design](#component-design)
4. [Class Diagrams](#class-diagrams)
5. [Implementation Plan](#implementation-plan)
6. [Security Considerations](#security-considerations)
7. [Testing Strategy](#testing-strategy)
8. [Error Handling](#error-handling)
9. [Integration Guide](#integration-guide)
10. [Performance Considerations](#performance-considerations)

---

## Executive Summary

This document defines the complete architecture for implementing legitimate DLL injection into the Lineage Remaster game client (Lin.bin). The injection is **required** by the client architecture and is not for malicious purposes.

### Key Requirements

- **Process Creation**: Create Lin.bin in SUSPENDED state
- **DLL Injection**: Inject 210916.asi, boxer.dll, libcocos2d.dll before execution
- **IPC Communication**: Named pipes for bidirectional launcher-game communication
- **Thread Safety**: Async/await patterns with proper synchronization
- **Error Recovery**: Graceful handling of injection failures
- **Integration**: Seamless integration with existing `ILauncherService` interface

### Design Principles

1. **Separation of Concerns**: Each component has a single responsibility
2. **RAII Pattern**: Proper resource cleanup via `IDisposable`
3. **Fail-Fast**: Early validation with clear error messages
4. **Testability**: Components designed for unit testing
5. **Maintainability**: Clear interfaces and documentation

---

## Architecture Overview

### High-Level Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      LinBinLauncher                         │
│              (ILauncherService Implementation)              │
│                                                             │
│  ┌───────────────────────────────────────────────────┐    │
│  │                LaunchGameAsync()                  │    │
│  └───────────────────┬───────────────────────────────┘    │
│                      │                                      │
│                      ▼                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           ProcessLaunchOrchestrator              │  │
│  │  (Coordinates all injection components)         │  │
│  └────┬─────────────────────────────────────────┬───┘  │
│       │                                         │        │
│       ▼                                         ▼        │
│  ┌─────────────┐                         ┌──────────┐   │
│  │ PipeManager │                         │ DllInjector│  │
│  │             │                         │            │  │
│  │ - Create    │                         │ - Inject   │  │
│  │ - Listen    │                         │ - Validate │  │
│  │ - Monitor   │                         │            │  │
│  └─────────────┘                         └──────┬─────┘  │
│       │                                         │        │
│       │                                         ▼        │
│       │                              ┌──────────────────┐│
│       │                              │ ProcessCreator   ││
│       │                              │                  ││
│       │                              │ - CreateSuspended││
│       │                              │ - Resume         ││
│       │                              │ - Terminate      ││
│       │                              └──────────────────┘│
│       │                                         │        │
│       └─────────────────┬───────────────────────┘        │
│                         ▼                                │
│              ┌────────────────────┐                      │
│              │ NativeInterop      │                      │
│              │                    │                      │
│              │ - Win32 API        │                      │
│              │ - P/Invoke         │                      │
│              │ - Structures       │                      │
│              └────────────────────┘                      │
└─────────────────────────────────────────────────────────────┘
```

### Process Flow Sequence

```
User
  │
  ├─► LinBinLauncher.LaunchGameAsync()
  │     │
  │     ├─► ProcessLaunchOrchestrator.LaunchWithInjectionAsync()
  │     │     │
  │     │     ├─► 1. PipeManager.CreatePipesAsync()
  │     │     │     ├─► Create "LineageLauncher_Pipe1" (Out)
  │     │     │     └─► Create "LineageLauncher_Pipe2" (In)
  │     │     │
  │     │     ├─► 2. ProcessCreator.CreateSuspendedAsync()
  │     │     │     ├─► NativeInterop.CreateProcess(CREATE_SUSPENDED)
  │     │     │     └─► Return Process Handle + Thread Handle
  │     │     │
  │     │     ├─► 3. DllInjector.InjectDllsAsync()
  │     │     │     │
  │     │     │     ├─► For each DLL (210916.asi, boxer.dll, libcocos2d.dll):
  │     │     │     │     │
  │     │     │     │     ├─► VirtualAllocEx (allocate memory)
  │     │     │     │     ├─► WriteProcessMemory (write DLL path)
  │     │     │     │     ├─► GetProcAddress(LoadLibraryA)
  │     │     │     │     ├─► CreateRemoteThread (call LoadLibraryA)
  │     │     │     │     └─► WaitForSingleObject (wait for completion)
  │     │     │     │
  │     │     │     └─► Validate all DLLs loaded
  │     │     │
  │     │     ├─► 4. ProcessCreator.ResumeMainThread()
  │     │     │     └─► NativeInterop.ResumeThread()
  │     │     │
  │     │     └─► 5. PipeManager.StartMonitoringAsync()
  │     │           └─► Listen for game communication
  │     │
  │     └─► Return Process ID
  │
  └─► Game Running with Injected DLLs
```

---

## Component Design

### 1. NativeInterop (Static Class)

**Responsibility**: P/Invoke declarations for Windows API functions.

**Location**: `src/LineageLauncher.Launcher/Native/NativeInterop.cs`

**Key Features**:
- All Win32 API function declarations
- Enums, flags, and structures
- Error handling wrappers
- Platform-specific code isolation

**Public Interface**:

```csharp
public static class NativeInterop
{
    // Process Creation
    public static bool CreateProcess(
        string applicationName,
        string commandLine,
        bool inheritHandles,
        ProcessCreationFlags creationFlags,
        string currentDirectory,
        out PROCESS_INFORMATION processInfo);

    // Memory Management
    public static IntPtr VirtualAllocEx(
        IntPtr processHandle,
        IntPtr address,
        uint size,
        AllocationType allocationType,
        MemoryProtection protection);

    public static bool WriteProcessMemory(
        IntPtr processHandle,
        IntPtr baseAddress,
        byte[] buffer,
        uint size,
        out int bytesWritten);

    public static bool VirtualFreeEx(
        IntPtr processHandle,
        IntPtr address,
        uint size,
        FreeType freeType);

    // Thread Management
    public static IntPtr CreateRemoteThread(
        IntPtr processHandle,
        IntPtr threadAttributes,
        uint stackSize,
        IntPtr startAddress,
        IntPtr parameter,
        uint creationFlags,
        out IntPtr threadId);

    public static uint ResumeThread(IntPtr threadHandle);
    public static uint SuspendThread(IntPtr threadHandle);

    // Module Information
    public static IntPtr GetModuleHandle(string moduleName);
    public static IntPtr GetProcAddress(IntPtr moduleHandle, string procName);

    // Handle Management
    public static bool CloseHandle(IntPtr handle);
    public static uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    // Error Handling
    public static int GetLastError();
    public static string GetLastErrorMessage();
}
```

**Structures**:

```csharp
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
    public string lpReserved;
    public string lpDesktop;
    public string lpTitle;
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
```

**Enums**:

```csharp
[Flags]
public enum ProcessCreationFlags : uint
{
    None = 0x00000000,
    DEBUG_PROCESS = 0x00000001,
    DEBUG_ONLY_THIS_PROCESS = 0x00000002,
    CREATE_SUSPENDED = 0x00000004,
    DETACHED_PROCESS = 0x00000008,
    CREATE_NEW_CONSOLE = 0x00000010,
    CREATE_UNICODE_ENVIRONMENT = 0x00000400,
}

[Flags]
public enum ProcessAccessFlags : uint
{
    All = 0x001F0FFF,
    Terminate = 0x00000001,
    CreateThread = 0x00000002,
    VMOperation = 0x00000008,
    VMRead = 0x00000010,
    VMWrite = 0x00000020,
    DupHandle = 0x00000040,
    SetInformation = 0x00000200,
    QueryInformation = 0x00000400,
    Synchronize = 0x00100000
}

[Flags]
public enum AllocationType : uint
{
    Commit = 0x1000,
    Reserve = 0x2000,
    Decommit = 0x4000,
    Release = 0x8000,
    Reset = 0x80000,
    Physical = 0x400000,
    TopDown = 0x100000,
    WriteWatch = 0x200000,
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
    Decommit = 0x4000,
    Release = 0x8000
}
```

---

### 2. ProcessCreator (Class)

**Responsibility**: Create and manage Lin.bin process lifecycle.

**Location**: `src/LineageLauncher.Launcher/Process/ProcessCreator.cs`

**Key Features**:
- Create process in suspended state
- Resume/suspend main thread
- Terminate process gracefully
- Handle cleanup on failure

**Public Interface**:

```csharp
public sealed class ProcessCreator : IDisposable
{
    public ProcessCreationResult CreateSuspended(
        string executablePath,
        string commandLine,
        string workingDirectory,
        IDictionary<string, string>? environmentVariables = null);

    public void ResumeMainThread();
    public void SuspendMainThread();
    public void Terminate();
    public bool IsRunning { get; }
    public int ProcessId { get; }
    public IntPtr ProcessHandle { get; }
    public IntPtr MainThreadHandle { get; }
    public void Dispose();
}

public sealed class ProcessCreationResult
{
    public bool Success { get; init; }
    public int ProcessId { get; init; }
    public IntPtr ProcessHandle { get; init; }
    public IntPtr ThreadHandle { get; init; }
    public string? ErrorMessage { get; init; }
}
```

**Implementation Details**:

```csharp
public sealed class ProcessCreator : IDisposable
{
    private readonly ILogger<ProcessCreator> _logger;
    private IntPtr _processHandle;
    private IntPtr _threadHandle;
    private int _processId;
    private bool _disposed;

    public ProcessCreator(ILogger<ProcessCreator> logger)
    {
        _logger = logger;
    }

    public ProcessCreationResult CreateSuspended(
        string executablePath,
        string commandLine,
        string workingDirectory,
        IDictionary<string, string>? environmentVariables = null)
    {
        // Validation
        if (!File.Exists(executablePath))
        {
            return new ProcessCreationResult
            {
                Success = false,
                ErrorMessage = $"Executable not found: {executablePath}"
            };
        }

        // Build command line with environment variables
        var fullCommandLine = BuildCommandLine(executablePath, commandLine);

        // Create process with CREATE_SUSPENDED flag
        var startupInfo = new STARTUPINFO { cb = Marshal.SizeOf<STARTUPINFO>() };
        var success = NativeInterop.CreateProcess(
            executablePath,
            fullCommandLine,
            inheritHandles: false,
            ProcessCreationFlags.CREATE_SUSPENDED | ProcessCreationFlags.CREATE_NEW_CONSOLE,
            workingDirectory,
            out var processInfo);

        if (!success)
        {
            var errorMessage = NativeInterop.GetLastErrorMessage();
            _logger.LogError("Failed to create process: {Error}", errorMessage);
            return new ProcessCreationResult
            {
                Success = false,
                ErrorMessage = $"CreateProcess failed: {errorMessage}"
            };
        }

        // Store handles
        _processHandle = processInfo.ProcessHandle;
        _threadHandle = processInfo.ThreadHandle;
        _processId = processInfo.ProcessId;

        _logger.LogInformation(
            "Process created in suspended state. PID: {ProcessId}",
            _processId);

        return new ProcessCreationResult
        {
            Success = true,
            ProcessId = _processId,
            ProcessHandle = _processHandle,
            ThreadHandle = _threadHandle
        };
    }

    public void ResumeMainThread()
    {
        if (_threadHandle == IntPtr.Zero)
            throw new InvalidOperationException("No thread to resume");

        var suspendCount = NativeInterop.ResumeThread(_threadHandle);
        _logger.LogInformation(
            "Main thread resumed. Previous suspend count: {SuspendCount}",
            suspendCount);
    }

    // Additional methods: Terminate, Dispose, etc.
}
```

---

### 3. DllInjector (Class)

**Responsibility**: Inject DLLs into suspended process.

**Location**: `src/LineageLauncher.Launcher/Injection/DllInjector.cs`

**Key Features**:
- Inject multiple DLLs sequentially
- Validate DLL paths before injection
- Wait for injection completion
- Cleanup memory on failure

**Public Interface**:

```csharp
public sealed class DllInjector : IDisposable
{
    public Task<DllInjectionResult> InjectDllsAsync(
        IntPtr processHandle,
        IEnumerable<string> dllPaths,
        CancellationToken cancellationToken = default);

    public Task<DllInjectionResult> InjectSingleDllAsync(
        IntPtr processHandle,
        string dllPath,
        CancellationToken cancellationToken = default);

    public void Dispose();
}

public sealed class DllInjectionResult
{
    public bool Success { get; init; }
    public int InjectedCount { get; init; }
    public List<string> InjectedDlls { get; init; } = new();
    public string? ErrorMessage { get; init; }
    public string? FailedDll { get; init; }
}
```

**Implementation Details**:

```csharp
public sealed class DllInjector : IDisposable
{
    private readonly ILogger<DllInjector> _logger;
    private readonly List<IntPtr> _allocatedMemory = new();
    private bool _disposed;

    public DllInjector(ILogger<DllInjector> logger)
    {
        _logger = logger;
    }

    public async Task<DllInjectionResult> InjectDllsAsync(
        IntPtr processHandle,
        IEnumerable<string> dllPaths,
        CancellationToken cancellationToken = default)
    {
        var result = new DllInjectionResult();

        foreach (var dllPath in dllPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Injecting DLL: {DllPath}", dllPath);

            var injectionResult = await InjectSingleDllAsync(
                processHandle,
                dllPath,
                cancellationToken);

            if (!injectionResult.Success)
            {
                result.Success = false;
                result.ErrorMessage = injectionResult.ErrorMessage;
                result.FailedDll = dllPath;
                return result;
            }

            result.InjectedDlls.Add(dllPath);
            result.InjectedCount++;
        }

        result.Success = true;
        _logger.LogInformation(
            "Successfully injected {Count} DLLs",
            result.InjectedCount);

        return result;
    }

    public async Task<DllInjectionResult> InjectSingleDllAsync(
        IntPtr processHandle,
        string dllPath,
        CancellationToken cancellationToken = default)
    {
        // Validate DLL exists
        if (!File.Exists(dllPath))
        {
            return new DllInjectionResult
            {
                Success = false,
                ErrorMessage = $"DLL not found: {dllPath}"
            };
        }

        // Get full path
        var fullDllPath = Path.GetFullPath(dllPath);
        var dllPathBytes = Encoding.Unicode.GetBytes(fullDllPath + "\0");

        // Step 1: Allocate memory in remote process
        var remoteMemory = NativeInterop.VirtualAllocEx(
            processHandle,
            IntPtr.Zero,
            (uint)dllPathBytes.Length,
            AllocationType.Commit | AllocationType.Reserve,
            MemoryProtection.ReadWrite);

        if (remoteMemory == IntPtr.Zero)
        {
            return new DllInjectionResult
            {
                Success = false,
                ErrorMessage = $"VirtualAllocEx failed: {NativeInterop.GetLastErrorMessage()}"
            };
        }

        _allocatedMemory.Add(remoteMemory);

        try
        {
            // Step 2: Write DLL path to remote memory
            var writeSuccess = NativeInterop.WriteProcessMemory(
                processHandle,
                remoteMemory,
                dllPathBytes,
                (uint)dllPathBytes.Length,
                out var bytesWritten);

            if (!writeSuccess || bytesWritten != dllPathBytes.Length)
            {
                return new DllInjectionResult
                {
                    Success = false,
                    ErrorMessage = $"WriteProcessMemory failed: {NativeInterop.GetLastErrorMessage()}"
                };
            }

            // Step 3: Get LoadLibraryW address
            var kernel32Handle = NativeInterop.GetModuleHandle("kernel32.dll");
            if (kernel32Handle == IntPtr.Zero)
            {
                return new DllInjectionResult
                {
                    Success = false,
                    ErrorMessage = "Failed to get kernel32.dll handle"
                };
            }

            var loadLibraryAddr = NativeInterop.GetProcAddress(kernel32Handle, "LoadLibraryW");
            if (loadLibraryAddr == IntPtr.Zero)
            {
                return new DllInjectionResult
                {
                    Success = false,
                    ErrorMessage = "Failed to get LoadLibraryW address"
                };
            }

            // Step 4: Create remote thread to call LoadLibraryW
            var threadHandle = NativeInterop.CreateRemoteThread(
                processHandle,
                IntPtr.Zero,
                0,
                loadLibraryAddr,
                remoteMemory,
                0,
                out var threadId);

            if (threadHandle == IntPtr.Zero)
            {
                return new DllInjectionResult
                {
                    Success = false,
                    ErrorMessage = $"CreateRemoteThread failed: {NativeInterop.GetLastErrorMessage()}"
                };
            }

            try
            {
                // Step 5: Wait for thread to complete (with timeout)
                var waitResult = await Task.Run(() =>
                    NativeInterop.WaitForSingleObject(threadHandle, 5000),
                    cancellationToken);

                if (waitResult != 0) // WAIT_OBJECT_0 = 0
                {
                    return new DllInjectionResult
                    {
                        Success = false,
                        ErrorMessage = $"Thread wait failed or timed out. Result: {waitResult}"
                    };
                }

                _logger.LogInformation("DLL injected successfully: {DllPath}", dllPath);
                return new DllInjectionResult { Success = true };
            }
            finally
            {
                NativeInterop.CloseHandle(threadHandle);
            }
        }
        finally
        {
            // Cleanup: Free remote memory
            NativeInterop.VirtualFreeEx(
                processHandle,
                remoteMemory,
                0,
                FreeType.Release);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Cleanup any remaining allocated memory
        foreach (var memory in _allocatedMemory)
        {
            // Note: Memory should already be freed, this is backup cleanup
        }

        _allocatedMemory.Clear();
        _disposed = true;
    }
}
```

---

### 4. PipeManager (Class)

**Responsibility**: Manage named pipes for IPC with game client.

**Location**: `src/LineageLauncher.Launcher/IPC/PipeManager.cs`

**Key Features**:
- Create bidirectional named pipes
- Async connection waiting
- Message send/receive
- Connection monitoring

**Public Interface**:

```csharp
public sealed class PipeManager : IDisposable
{
    public Task CreatePipesAsync(CancellationToken cancellationToken = default);
    public Task WaitForConnectionAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    public Task<byte[]?> ReadMessageAsync(CancellationToken cancellationToken = default);
    public Task WriteMessageAsync(byte[] data, CancellationToken cancellationToken = default);
    public bool IsConnected { get; }
    public void Dispose();
}
```

**Implementation Details**:

```csharp
public sealed class PipeManager : IDisposable
{
    private readonly ILogger<PipeManager> _logger;
    private readonly string _pipeNameOut = "LineageLauncher_Pipe1";
    private readonly string _pipeNameIn = "LineageLauncher_Pipe2";

    private NamedPipeServerStream? _pipeOut;
    private NamedPipeServerStream? _pipeIn;
    private bool _disposed;

    public bool IsConnected =>
        _pipeOut?.IsConnected == true && _pipeIn?.IsConnected == true;

    public PipeManager(ILogger<PipeManager> logger)
    {
        _logger = logger;
    }

    public Task CreatePipesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating named pipes...");

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
            throw;
        }
    }

    public async Task WaitForConnectionAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (_pipeOut == null || _pipeIn == null)
            throw new InvalidOperationException("Pipes not created");

        _logger.LogInformation("Waiting for game client connection...");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            // Wait for both pipes to connect
            await Task.WhenAll(
                _pipeOut.WaitForConnectionAsync(cts.Token),
                _pipeIn.WaitForConnectionAsync(cts.Token));

            _logger.LogInformation("Game client connected to pipes");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Pipe connection timeout after {Timeout}", timeout);
            throw new TimeoutException($"Game client did not connect within {timeout}");
        }
    }

    public async Task<byte[]?> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        if (_pipeIn == null || !_pipeIn.IsConnected)
            return null;

        var buffer = new byte[4096];
        var bytesRead = await _pipeIn.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        if (bytesRead == 0)
            return null;

        var message = new byte[bytesRead];
        Array.Copy(buffer, message, bytesRead);
        return message;
    }

    public async Task WriteMessageAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (_pipeOut == null || !_pipeOut.IsConnected)
            throw new InvalidOperationException("Pipe not connected");

        await _pipeOut.WriteAsync(data, 0, data.Length, cancellationToken);
        await _pipeOut.FlushAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _pipeOut?.Dispose();
        _pipeIn?.Dispose();
        _disposed = true;

        _logger.LogInformation("Pipes disposed");
    }
}
```

---

### 5. ProcessLaunchOrchestrator (Class)

**Responsibility**: Coordinate entire launch process with injection.

**Location**: `src/LineageLauncher.Launcher/Orchestration/ProcessLaunchOrchestrator.cs`

**Key Features**:
- Orchestrate all components
- Handle rollback on failure
- Monitor process health
- Provide status updates

**Public Interface**:

```csharp
public sealed class ProcessLaunchOrchestrator : IDisposable
{
    public Task<LaunchResult> LaunchWithInjectionAsync(
        LaunchConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);

    public Task TerminateAsync();
    public bool IsProcessRunning { get; }
    public int ProcessId { get; }
    public void Dispose();
}

public sealed class LaunchConfiguration
{
    public required string ExecutablePath { get; init; }
    public required string WorkingDirectory { get; init; }
    public required List<string> DllsToInject { get; init; }
    public IDictionary<string, string>? EnvironmentVariables { get; init; }
    public TimeSpan PipeConnectionTimeout { get; init; } = TimeSpan.FromSeconds(10);
}

public sealed class LaunchResult
{
    public bool Success { get; init; }
    public int ProcessId { get; init; }
    public string? ErrorMessage { get; init; }
    public LaunchPhase FailedPhase { get; init; }
}

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

**Implementation Details**:

```csharp
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
        _logger = logger;
        _processCreator = processCreator;
        _dllInjector = dllInjector;
        _pipeManager = pipeManager;
    }

    public async Task<LaunchResult> LaunchWithInjectionAsync(
        LaunchConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting launch with injection...");

        try
        {
            // Phase 1: Create Named Pipes
            progress?.Report("Creating IPC pipes...");
            await _pipeManager.CreatePipesAsync(cancellationToken);
            _logger.LogInformation("Phase 1: Pipes created");

            // Phase 2: Create Process (Suspended)
            progress?.Report("Creating game process...");
            var createResult = _processCreator.CreateSuspended(
                config.ExecutablePath,
                commandLine: string.Empty,
                config.WorkingDirectory,
                config.EnvironmentVariables);

            if (!createResult.Success)
            {
                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = createResult.ErrorMessage,
                    FailedPhase = LaunchPhase.ProcessCreation
                };
            }
            _logger.LogInformation("Phase 2: Process created (PID: {ProcessId})", createResult.ProcessId);

            // Phase 3: Inject DLLs
            progress?.Report("Injecting required DLLs...");
            var injectionResult = await _dllInjector.InjectDllsAsync(
                createResult.ProcessHandle,
                config.DllsToInject,
                cancellationToken);

            if (!injectionResult.Success)
            {
                // Rollback: Terminate process
                _processCreator.Terminate();

                return new LaunchResult
                {
                    Success = false,
                    ErrorMessage = $"DLL injection failed: {injectionResult.ErrorMessage}",
                    FailedPhase = LaunchPhase.DllInjection
                };
            }
            _logger.LogInformation("Phase 3: DLLs injected ({Count} DLLs)", injectionResult.InjectedCount);

            // Phase 4: Resume Main Thread
            progress?.Report("Starting game...");
            _processCreator.ResumeMainThread();
            _logger.LogInformation("Phase 4: Main thread resumed");

            // Phase 5: Wait for Pipe Connection (Optional)
            progress?.Report("Waiting for game initialization...");
            try
            {
                await _pipeManager.WaitForConnectionAsync(
                    config.PipeConnectionTimeout,
                    cancellationToken);
                _logger.LogInformation("Phase 5: Pipe connected");
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Game did not connect to pipes within timeout (this may be normal)");
            }

            progress?.Report("Game launched successfully");
            return new LaunchResult
            {
                Success = true,
                ProcessId = createResult.ProcessId
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Launch cancelled");
            await TerminateAsync();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Launch failed with unexpected error");
            await TerminateAsync();
            return new LaunchResult
            {
                Success = false,
                ErrorMessage = $"Unexpected error: {ex.Message}",
                FailedPhase = LaunchPhase.None
            };
        }
    }

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
    }
}
```

---

## Implementation Plan

### Phase 1: Foundation (Days 1-2)

**Goal**: Set up native interop and structures

**Files to Create**:

1. `src/LineageLauncher.Launcher/Native/NativeInterop.cs`
   - All P/Invoke declarations
   - Error handling wrappers
   - ~400 lines

2. `src/LineageLauncher.Launcher/Native/NativeStructures.cs`
   - PROCESS_INFORMATION
   - STARTUPINFO
   - All enums
   - ~200 lines

3. `src/LineageLauncher.Launcher/Native/NativeConstants.cs`
   - Timeout constants
   - Wait result codes
   - ~50 lines

**Testing**: Unit tests for structure marshalling

**Deliverable**: Compiles without errors, all Win32 APIs declared

---

### Phase 2: Process Management (Days 3-4)

**Goal**: Create and manage suspended processes

**Files to Create**:

1. `src/LineageLauncher.Launcher/Process/ProcessCreator.cs`
   - Full implementation
   - ~300 lines

2. `src/LineageLauncher.Launcher/Process/ProcessCreationResult.cs`
   - Result class
   - ~30 lines

**Testing**:
- Create suspended notepad.exe
- Verify process is suspended
- Resume and verify it runs
- Terminate and verify cleanup

**Deliverable**: Can create/resume/terminate any process

---

### Phase 3: DLL Injection (Days 5-6)

**Goal**: Inject DLLs into suspended process

**Files to Create**:

1. `src/LineageLauncher.Launcher/Injection/DllInjector.cs`
   - Full implementation
   - ~400 lines

2. `src/LineageLauncher.Launcher/Injection/DllInjectionResult.cs`
   - Result class
   - ~30 lines

**Testing**:
- Inject test DLL into suspended notepad.exe
- Verify DLL loads (check modules)
- Test error handling (invalid DLL path)
- Test multiple DLL injection

**Deliverable**: Can inject any DLL into any process

---

### Phase 4: IPC Pipes (Days 7-8)

**Goal**: Named pipe communication

**Files to Create**:

1. `src/LineageLauncher.Launcher/IPC/PipeManager.cs`
   - Full implementation
   - ~250 lines

**Testing**:
- Create pipes
- Test connection from test client
- Send/receive messages
- Test timeout handling

**Deliverable**: Bidirectional pipe communication works

---

### Phase 5: Orchestration (Days 9-10)

**Goal**: Coordinate all components

**Files to Create**:

1. `src/LineageLauncher.Launcher/Orchestration/ProcessLaunchOrchestrator.cs`
   - Full implementation
   - ~350 lines

2. `src/LineageLauncher.Launcher/Orchestration/LaunchConfiguration.cs`
   - Config classes
   - ~50 lines

**Testing**:
- Full launch flow with test executable
- Error handling at each phase
- Rollback on failure
- Resource cleanup

**Deliverable**: Complete orchestration works

---

### Phase 6: Integration (Day 11)

**Goal**: Integrate with LinBinLauncher

**Files to Modify**:

1. `src/LineageLauncher.Launcher/LinBinLauncher.cs`
   - Replace `Process.Start()` with orchestrator
   - Add dependency injection
   - ~50 lines changed

**Testing**:
- Launch Lin.bin with real DLLs
- Verify game starts properly
- Test multiple launches
- Test error scenarios

**Deliverable**: Lin.bin launches successfully

---

## Integration Guide

### Modifying LinBinLauncher.cs

**Current Code** (lines 96-122):

```csharp
var startInfo = new ProcessStartInfo
{
    FileName = clientPath,
    WorkingDirectory = clientDirectory,
    UseShellExecute = false,
    RedirectStandardOutput = false,
    RedirectStandardError = false,
    CreateNoWindow = false
};

startInfo.EnvironmentVariables["L1_DLL_PASSWORD"] = dllPassword.ToString();
startInfo.EnvironmentVariables["L1_CLIENT_SIDE_KEY"] = clientSideKey.ToString();

_gameProcess = Process.Start(startInfo);
```

**New Code** (with injection):

```csharp
// Create orchestrator with dependencies
using var orchestrator = new ProcessLaunchOrchestrator(
    _logger,
    new ProcessCreator(_logger),
    new DllInjector(_logger),
    new PipeManager(_logger));

// Configure launch
var config = new LaunchConfiguration
{
    ExecutablePath = clientPath,
    WorkingDirectory = clientDirectory,
    DllsToInject = new List<string>
    {
        Path.Combine(clientDirectory, "210916.asi"),
        Path.Combine(clientDirectory, "boxer.dll"),
        Path.Combine(clientDirectory, "libcocos2d.dll")
    },
    EnvironmentVariables = new Dictionary<string, string>
    {
        ["L1_DLL_PASSWORD"] = dllPassword.ToString(),
        ["L1_CLIENT_SIDE_KEY"] = clientSideKey.ToString()
    },
    PipeConnectionTimeout = TimeSpan.FromSeconds(10)
};

// Launch with injection
var progress = new Progress<string>(msg => _logger.LogInformation(msg));
var launchResult = await orchestrator.LaunchWithInjectionAsync(
    config,
    progress,
    cancellationToken);

if (!launchResult.Success)
{
    _logger.LogError(
        "Launch failed at phase {Phase}: {Error}",
        launchResult.FailedPhase,
        launchResult.ErrorMessage);
    return false;
}

_gameProcess = Process.GetProcessById(launchResult.ProcessId);
```

### Dependency Injection Setup

In `Program.cs` or DI container:

```csharp
services.AddTransient<ProcessCreator>();
services.AddTransient<DllInjector>();
services.AddTransient<PipeManager>();
services.AddTransient<ProcessLaunchOrchestrator>();
services.AddSingleton<ILauncherService, LinBinLauncher>();
```

---

## Security Considerations

### 1. Anti-Cheat Detection

**Risk**: DLL injection is commonly used by cheats

**Mitigation**:
- Inject ONLY official DLLs (210916.asi, boxer.dll, libcocos2d.dll)
- Inject BEFORE anti-cheat initializes (suspended process)
- Use legitimate LoadLibraryW (not manual mapping)
- Sign launcher executable with code signing certificate

### 2. DLL Validation

**Before Injection**:
```csharp
private bool ValidateDll(string dllPath)
{
    // Check file exists
    if (!File.Exists(dllPath))
        return false;

    // Verify digital signature (optional but recommended)
    var cert = X509Certificate.CreateFromSignedFile(dllPath);
    if (cert == null)
        _logger.LogWarning("DLL is not signed: {DllPath}", dllPath);

    // Check file hash against known good values
    var hash = ComputeFileHash(dllPath);
    if (!_knownGoodHashes.Contains(hash))
    {
        _logger.LogError("DLL hash mismatch: {DllPath}", dllPath);
        return false;
    }

    return true;
}
```

### 3. Process Termination on Failure

Always terminate suspended process if injection fails:

```csharp
try
{
    var injectionResult = await _dllInjector.InjectDllsAsync(...);
    if (!injectionResult.Success)
    {
        _processCreator.Terminate(); // Critical!
        return LaunchResult.Failed(...);
    }
}
catch
{
    _processCreator.Terminate(); // Always cleanup
    throw;
}
```

### 4. Handle Leaks

Use `IDisposable` pattern to ensure handles are closed:

```csharp
public void Dispose()
{
    if (_processHandle != IntPtr.Zero)
    {
        NativeInterop.CloseHandle(_processHandle);
        _processHandle = IntPtr.Zero;
    }

    if (_threadHandle != IntPtr.Zero)
    {
        NativeInterop.CloseHandle(_threadHandle);
        _threadHandle = IntPtr.Zero;
    }
}
```

---

## Error Handling

### Error Categories

1. **Validation Errors**: File not found, invalid path
2. **API Errors**: CreateProcess failed, VirtualAllocEx failed
3. **Timeout Errors**: Thread wait timeout, pipe connection timeout
4. **Security Errors**: DLL signature invalid, hash mismatch

### Error Handling Strategy

```csharp
public async Task<LaunchResult> LaunchWithInjectionAsync(...)
{
    try
    {
        // Phase 1: Validate inputs
        ValidateConfiguration(config);

        // Phase 2-5: Execute with try-catch per phase
        return await ExecuteLaunchAsync(config, cancellationToken);
    }
    catch (ValidationException ex)
    {
        _logger.LogError(ex, "Validation failed");
        return LaunchResult.Failed(LaunchPhase.None, ex.Message);
    }
    catch (Win32Exception ex)
    {
        _logger.LogError(ex, "Windows API call failed: {ErrorCode}", ex.NativeErrorCode);
        return LaunchResult.Failed(currentPhase, $"API Error: {ex.Message}");
    }
    catch (TimeoutException ex)
    {
        _logger.LogWarning(ex, "Operation timed out");
        return LaunchResult.Failed(currentPhase, ex.Message);
    }
    catch (OperationCanceledException)
    {
        _logger.LogInformation("Operation cancelled by user");
        throw; // Propagate cancellation
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during launch");
        return LaunchResult.Failed(LaunchPhase.None, $"Unexpected: {ex.Message}");
    }
    finally
    {
        // Always cleanup resources
        CleanupResources();
    }
}
```

### User-Facing Error Messages

```csharp
private static string GetUserFriendlyError(LaunchResult result)
{
    return result.FailedPhase switch
    {
        LaunchPhase.ProcessCreation =>
            "Failed to start the game. Please check that Lin.bin exists and is not corrupted.",

        LaunchPhase.DllInjection =>
            $"Failed to load required game components: {result.ErrorMessage}\n" +
            "Please verify game files integrity.",

        LaunchPhase.ThreadResume =>
            "Game process could not be started. Please restart the launcher.",

        LaunchPhase.PipeConnection =>
            "Game started but communication failed. This may be normal, check if game is running.",

        _ =>
            $"An unexpected error occurred: {result.ErrorMessage}"
    };
}
```

---

## Testing Strategy

### Phase 1: Unit Tests

**Test Native Interop**:

```csharp
[Fact]
public void CreateProcess_WithValidPath_ReturnsSuccess()
{
    var result = NativeInterop.CreateProcess(
        "C:\\Windows\\notepad.exe",
        "",
        false,
        ProcessCreationFlags.CREATE_SUSPENDED,
        "C:\\Windows",
        out var processInfo);

    Assert.True(result);
    Assert.NotEqual(IntPtr.Zero, processInfo.ProcessHandle);

    // Cleanup
    NativeInterop.CloseHandle(processInfo.ProcessHandle);
    NativeInterop.CloseHandle(processInfo.ThreadHandle);
}
```

**Test Structure Marshalling**:

```csharp
[Fact]
public void STARTUPINFO_HasCorrectSize()
{
    var info = new STARTUPINFO { cb = Marshal.SizeOf<STARTUPINFO>() };
    Assert.Equal(68, info.cb); // Expected size on x64
}
```

### Phase 2: Integration Tests

**Test ProcessCreator**:

```csharp
[Fact]
public async Task ProcessCreator_CreateAndResume_Success()
{
    using var creator = new ProcessCreator(_logger);

    var result = creator.CreateSuspended(
        "C:\\Windows\\notepad.exe",
        "",
        "C:\\Windows");

    Assert.True(result.Success);
    Assert.True(creator.IsRunning);

    creator.ResumeMainThread();

    await Task.Delay(100);
    Assert.True(creator.IsRunning);

    creator.Terminate();
    Assert.False(creator.IsRunning);
}
```

**Test DllInjector**:

```csharp
[Fact]
public async Task DllInjector_InjectTestDll_Success()
{
    // Create suspended process
    using var creator = new ProcessCreator(_logger);
    var createResult = creator.CreateSuspended(...);

    // Inject test DLL
    using var injector = new DllInjector(_logger);
    var injectResult = await injector.InjectSingleDllAsync(
        createResult.ProcessHandle,
        "TestDlls\\test.dll");

    Assert.True(injectResult.Success);

    // Resume and verify DLL loaded
    creator.ResumeMainThread();

    // Check loaded modules (requires external tool or API)
    var modules = GetProcessModules(createResult.ProcessId);
    Assert.Contains("test.dll", modules);
}
```

### Phase 3: Manual Tests

**Test with Lin.bin**:

1. **Normal Launch**:
   - Launch Lin.bin with all 3 DLLs
   - Verify game starts
   - Check game logs for errors
   - Verify DLLs are loaded (Process Explorer)

2. **Missing DLL**:
   - Remove one DLL
   - Verify injection fails gracefully
   - Verify process is terminated
   - Verify error message is clear

3. **Invalid DLL Path**:
   - Provide wrong DLL path
   - Verify validation catches it
   - Verify no process is created

4. **Multiple Launches**:
   - Launch game 5 times sequentially
   - Verify no resource leaks
   - Check handle count doesn't grow

5. **Cancellation**:
   - Start launch
   - Cancel during injection phase
   - Verify process is terminated
   - Verify no orphaned processes

### Phase 4: Performance Tests

**Measure Launch Time**:

```csharp
[Fact]
public async Task LaunchPerformance_MeasureTime()
{
    var stopwatch = Stopwatch.StartNew();

    var result = await _orchestrator.LaunchWithInjectionAsync(config);

    stopwatch.Stop();
    _logger.LogInformation("Launch took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

    // Should complete in under 2 seconds
    Assert.True(stopwatch.ElapsedMilliseconds < 2000);
}
```

**Measure Memory Usage**:

```csharp
[Fact]
public async Task LaunchMemory_NoLeaks()
{
    var initialMemory = GC.GetTotalMemory(true);

    for (int i = 0; i < 10; i++)
    {
        using var orchestrator = CreateOrchestrator();
        await orchestrator.LaunchWithInjectionAsync(config);
        await orchestrator.TerminateAsync();
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    var finalMemory = GC.GetTotalMemory(true);
    var leak = finalMemory - initialMemory;

    _logger.LogInformation("Memory delta: {Leak} bytes", leak);
    Assert.True(leak < 1_000_000); // Less than 1MB leak
}
```

---

## Performance Considerations

### 1. Injection Speed

**Optimization**: Inject DLLs in parallel (if safe)

```csharp
public async Task<DllInjectionResult> InjectDllsParallelAsync(
    IntPtr processHandle,
    IEnumerable<string> dllPaths,
    CancellationToken cancellationToken = default)
{
    var tasks = dllPaths.Select(path =>
        InjectSingleDllAsync(processHandle, path, cancellationToken));

    var results = await Task.WhenAll(tasks);

    // Check if all succeeded
    if (results.All(r => r.Success))
        return DllInjectionResult.Success(results.Length);

    var failed = results.First(r => !r.Success);
    return DllInjectionResult.Failed(failed.ErrorMessage);
}
```

**Note**: Parallel injection may cause issues if DLLs depend on each other. Test thoroughly.

### 2. Memory Allocation

**Reuse Buffers**:

```csharp
private readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

public async Task<byte[]?> ReadMessageAsync(CancellationToken cancellationToken)
{
    var buffer = _bufferPool.Rent(4096);
    try
    {
        var bytesRead = await _pipeIn.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        if (bytesRead == 0) return null;

        var message = new byte[bytesRead];
        Array.Copy(buffer, message, bytesRead);
        return message;
    }
    finally
    {
        _bufferPool.Return(buffer);
    }
}
```

### 3. Handle Management

**Minimize Handle Lifetime**:

```csharp
public DllInjectionResult InjectSingleDll(IntPtr processHandle, string dllPath)
{
    IntPtr remoteMemory = IntPtr.Zero;
    IntPtr threadHandle = IntPtr.Zero;

    try
    {
        remoteMemory = VirtualAllocEx(...);
        // ... injection code ...
        threadHandle = CreateRemoteThread(...);
        WaitForSingleObject(threadHandle, TIMEOUT);

        return DllInjectionResult.Success();
    }
    finally
    {
        // Always cleanup immediately
        if (remoteMemory != IntPtr.Zero)
            VirtualFreeEx(processHandle, remoteMemory, 0, FreeType.Release);

        if (threadHandle != IntPtr.Zero)
            CloseHandle(threadHandle);
    }
}
```

### 4. Logging Performance

**Structured Logging**:

```csharp
// Bad: String interpolation creates allocations even if not logged
_logger.LogDebug($"Injecting DLL: {dllPath}");

// Good: Uses message template
_logger.LogDebug("Injecting DLL: {DllPath}", dllPath);
```

---

## Summary

This architecture provides a robust, maintainable solution for DLL injection into Lin.bin:

### Key Strengths

1. **Separation of Concerns**: Each component has a single, well-defined responsibility
2. **Error Handling**: Comprehensive error handling at every layer
3. **Resource Management**: Proper cleanup via IDisposable pattern
4. **Testability**: Components can be unit tested independently
5. **Extensibility**: Easy to add new features (e.g., additional IPC protocols)
6. **Maintainability**: Clear interfaces and extensive documentation

### Project Files

**New Files** (7 files):
1. `src/LineageLauncher.Launcher/Native/NativeInterop.cs` (~400 lines)
2. `src/LineageLauncher.Launcher/Native/NativeStructures.cs` (~200 lines)
3. `src/LineageLauncher.Launcher/Process/ProcessCreator.cs` (~300 lines)
4. `src/LineageLauncher.Launcher/Injection/DllInjector.cs` (~400 lines)
5. `src/LineageLauncher.Launcher/IPC/PipeManager.cs` (~250 lines)
6. `src/LineageLauncher.Launcher/Orchestration/ProcessLaunchOrchestrator.cs` (~350 lines)
7. `src/LineageLauncher.Launcher/Orchestration/LaunchConfiguration.cs` (~50 lines)

**Modified Files** (1 file):
1. `src/LineageLauncher.Launcher/LinBinLauncher.cs` (~50 lines changed)

**Total New Code**: ~2,000 lines

### Implementation Timeline

- **Phase 1-2**: Foundation (Days 1-4)
- **Phase 3-4**: Core Functionality (Days 5-8)
- **Phase 5-6**: Integration (Days 9-11)
- **Total**: 11 days for complete implementation and testing

### Next Steps

1. Review and approve architecture
2. Create unit test project structure
3. Implement Phase 1 (NativeInterop)
4. Implement Phase 2 (ProcessCreator)
5. Continue through phases sequentially
6. Integration testing with real Lin.bin
7. Performance optimization
8. Production deployment

---

**Document Version**: 1.0
**Last Updated**: 2025-11-12
**Status**: Ready for Implementation
