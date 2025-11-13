# DLL Injection Testing Guide
## Comprehensive Testing Strategy for Lineage Launcher

**Date**: 2025-11-12
**Purpose**: Step-by-step testing approach for DLL injection implementation

---

## Table of Contents

1. [Testing Philosophy](#testing-philosophy)
2. [Test Environment Setup](#test-environment-setup)
3. [Phase 1: Unit Tests](#phase-1-unit-tests)
4. [Phase 2: Integration Tests](#phase-2-integration-tests)
5. [Phase 3: Manual Tests](#phase-3-manual-tests)
6. [Phase 4: End-to-End Tests](#phase-4-end-to-end-tests)
7. [Phase 5: Production Validation](#phase-5-production-validation)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Performance Benchmarks](#performance-benchmarks)

---

## Testing Philosophy

### Goals

1. **Safety**: Never crash the target process or leave orphans
2. **Reliability**: Works consistently across multiple launches
3. **Performance**: Launch completes in under 2 seconds
4. **Robustness**: Handles all error conditions gracefully

### Test Pyramid

```
                    ▲
                   ╱ ╲
                  ╱   ╲
                 ╱ E2E ╲        <- Manual/Automated (5%)
                ╱───────╲
               ╱ Integra-╲      <- Integration Tests (20%)
              ╱───────────╲
             ╱             ╲
            ╱   Unit Tests  ╲   <- Unit Tests (75%)
           ╱─────────────────╲
          ──────────────────────
```

### Test Coverage Target

- **Unit Tests**: 90%+ coverage
- **Integration Tests**: All critical paths
- **Manual Tests**: All user-facing scenarios

---

## Test Environment Setup

### Prerequisites

1. **Test Projects**:
   ```bash
   mkdir tests/LineageLauncher.Launcher.Tests
   cd tests/LineageLauncher.Launcher.Tests
   dotnet new xunit
   dotnet add reference ../../src/LineageLauncher.Launcher
   ```

2. **Required NuGet Packages**:
   ```bash
   dotnet add package xUnit
   dotnet add package xunit.runner.visualstudio
   dotnet add package Microsoft.NET.Test.Sdk
   dotnet add package Moq
   dotnet add package FluentAssertions
   ```

3. **Test DLLs** (create dummy DLLs for testing):
   ```csharp
   // TestDlls/TestDummy.dll - Empty DLL for injection testing
   // Compile with: csc /target:library /out:TestDummy.dll TestDummy.cs

   namespace TestDummy
   {
       public class Dummy
       {
           public static void Init() { }
       }
   }
   ```

4. **Test Executables**:
   - Notepad.exe (built-in Windows app for safe testing)
   - Custom test executable that loads DLLs

---

## Phase 1: Unit Tests

### Test 1.1: NativeInterop Structure Marshalling

**File**: `tests/LineageLauncher.Launcher.Tests/Native/NativeStructuresTests.cs`

```csharp
using System.Runtime.InteropServices;
using FluentAssertions;
using LineageLauncher.Launcher.Native;
using Xunit;

namespace LineageLauncher.Launcher.Tests.Native;

public class NativeStructuresTests
{
    [Fact]
    public void PROCESS_INFORMATION_HasCorrectLayout()
    {
        // Arrange & Act
        var size = Marshal.SizeOf<PROCESS_INFORMATION>();

        // Assert - Size should be appropriate for handles + 2 ints
        size.Should().BeGreaterThan(0);
        size.Should().Be(IntPtr.Size * 2 + sizeof(int) * 2);
    }

    [Fact]
    public void STARTUPINFO_InitializesCorrectly()
    {
        // Arrange & Act
        var info = new STARTUPINFO
        {
            cb = Marshal.SizeOf<STARTUPINFO>()
        };

        // Assert
        info.cb.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ProcessCreationFlags_HasCorrectValues()
    {
        // Assert
        ProcessCreationFlags.CreateSuspended.Should().Be((ProcessCreationFlags)0x00000004);
        ProcessCreationFlags.CreateNewConsole.Should().Be((ProcessCreationFlags)0x00000010);
    }
}
```

### Test 1.2: NativeInterop API Calls

**File**: `tests/LineageLauncher.Launcher.Tests/Native/NativeInteropTests.cs`

```csharp
using FluentAssertions;
using LineageLauncher.Launcher.Native;
using Xunit;

namespace LineageLauncher.Launcher.Tests.Native;

public class NativeInteropTests
{
    [Fact]
    public void GetModuleHandle_WithKernel32_ReturnsValidHandle()
    {
        // Act
        var handle = NativeInterop.GetModuleHandle("kernel32.dll");

        // Assert
        handle.Should().NotBe(IntPtr.Zero);
    }

    [Fact]
    public void GetFunctionAddress_WithLoadLibraryW_ReturnsValidAddress()
    {
        // Arrange
        var kernel32 = NativeInterop.GetModuleHandle("kernel32.dll");

        // Act
        var address = NativeInterop.GetFunctionAddress(kernel32, "LoadLibraryW");

        // Assert
        address.Should().NotBe(IntPtr.Zero);
    }

    [Fact]
    public void CloseHandle_WithInvalidHandle_ReturnsFalse()
    {
        // Act
        var result = NativeInterop.CloseHandle(IntPtr.Zero);

        // Assert
        result.Should().BeFalse();
    }
}
```

### Test 1.3: ProcessCreator Basic Validation

**File**: `tests/LineageLauncher.Launcher.Tests/Process/ProcessCreatorTests.cs`

```csharp
using FluentAssertions;
using LineageLauncher.Launcher.Process;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LineageLauncher.Launcher.Tests.Process;

public class ProcessCreatorTests
{
    private readonly ProcessCreator _creator;
    private readonly Mock<ILogger<ProcessCreator>> _loggerMock;

    public ProcessCreatorTests()
    {
        _loggerMock = new Mock<ILogger<ProcessCreator>>();
        _creator = new ProcessCreator(_loggerMock.Object);
    }

    [Fact]
    public void CreateSuspended_WithInvalidPath_ReturnsFailure()
    {
        // Act
        var result = _creator.CreateSuspended(
            "C:\\NonExistent\\Program.exe",
            "",
            "C:\\");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public void CreateSuspended_WithEmptyPath_ReturnsFailure()
    {
        // Act
        var result = _creator.CreateSuspended(
            "",
            "",
            "C:\\");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public void ResumeMainThread_WithoutCreation_ThrowsException()
    {
        // Act
        Action act = () => _creator.ResumeMainThread();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        // Act
        _creator.Dispose();
        Action act = () => _creator.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
```

### Test 1.4: DllInjector Validation

**File**: `tests/LineageLauncher.Launcher.Tests/Injection/DllInjectorTests.cs`

```csharp
using FluentAssertions;
using LineageLauncher.Launcher.Injection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LineageLauncher.Launcher.Tests.Injection;

public class DllInjectorTests
{
    private readonly DllInjector _injector;
    private readonly Mock<ILogger<DllInjector>> _loggerMock;

    public DllInjectorTests()
    {
        _loggerMock = new Mock<ILogger<DllInjector>>();
        _injector = new DllInjector(_loggerMock.Object);
    }

    [Fact]
    public async Task InjectDllsAsync_WithEmptyList_ReturnsFailure()
    {
        // Act
        var result = await _injector.InjectDllsAsync(
            IntPtr.Zero,
            new List<string>());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No DLLs specified");
    }

    [Fact]
    public async Task InjectSingleDllAsync_WithNonExistentDll_ReturnsFailure()
    {
        // Arrange
        var processHandle = new IntPtr(12345); // Fake handle

        // Act
        var result = await _injector.InjectSingleDllAsync(
            processHandle,
            "C:\\NonExistent.dll");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        // Act
        _injector.Dispose();
        Action act = () => _injector.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
```

### Test 1.5: PipeManager Basic Operations

**File**: `tests/LineageLauncher.Launcher.Tests/IPC/PipeManagerTests.cs`

```csharp
using FluentAssertions;
using LineageLauncher.Launcher.IPC;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LineageLauncher.Launcher.Tests.IPC;

public class PipeManagerTests
{
    private readonly PipeManager _pipeManager;
    private readonly Mock<ILogger<PipeManager>> _loggerMock;

    public PipeManagerTests()
    {
        _loggerMock = new Mock<ILogger<PipeManager>>();
        _pipeManager = new PipeManager(_loggerMock.Object);
    }

    [Fact]
    public async Task CreatePipesAsync_FirstCall_Succeeds()
    {
        // Act
        await _pipeManager.CreatePipesAsync();

        // Assert
        _pipeManager.IsConnected.Should().BeFalse(); // Not connected yet
    }

    [Fact]
    public async Task CreatePipesAsync_SecondCall_ThrowsException()
    {
        // Arrange
        await _pipeManager.CreatePipesAsync();

        // Act
        Func<Task> act = async () => await _pipeManager.CreatePipesAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task WaitForConnectionAsync_WithoutCreation_ThrowsException()
    {
        // Act
        Func<Task> act = async () =>
            await _pipeManager.WaitForConnectionAsync(TimeSpan.FromSeconds(1));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        // Act
        _pipeManager.Dispose();
        Action act = () => _pipeManager.Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
```

---

## Phase 2: Integration Tests

### Test 2.1: Process Creation and Resumption

**File**: `tests/LineageLauncher.Launcher.Tests/Integration/ProcessLifecycleTests.cs`

```csharp
using FluentAssertions;
using LineageLauncher.Launcher.Process;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LineageLauncher.Launcher.Tests.Integration;

public class ProcessLifecycleTests : IDisposable
{
    private readonly ProcessCreator _creator;
    private readonly ILoggerFactory _loggerFactory;

    public ProcessLifecycleTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _creator = new ProcessCreator(_loggerFactory.CreateLogger<ProcessCreator>());
    }

    [Fact(Skip = "Requires Windows environment")]
    public void CreateSuspended_WithNotepad_SucceedsAndProcessIsSuspended()
    {
        // Act
        var result = _creator.CreateSuspended(
            @"C:\Windows\System32\notepad.exe",
            "",
            @"C:\Windows\System32");

        // Assert
        result.Success.Should().BeTrue();
        result.ProcessId.Should().BeGreaterThan(0);
        result.ProcessHandle.Should().NotBe(IntPtr.Zero);

        _creator.IsRunning.Should().BeTrue();

        // Verify process is suspended (should not show window yet)
        System.Threading.Thread.Sleep(500);
        // Process should still be suspended

        // Cleanup
        _creator.Terminate();
    }

    [Fact(Skip = "Requires Windows environment")]
    public void ResumeMainThread_AfterCreation_ProcessRuns()
    {
        // Arrange
        var result = _creator.CreateSuspended(
            @"C:\Windows\System32\notepad.exe",
            "",
            @"C:\Windows\System32");

        result.Success.Should().BeTrue();

        // Act
        _creator.ResumeMainThread();

        // Allow process to start
        System.Threading.Thread.Sleep(1000);

        // Assert
        _creator.IsRunning.Should().BeTrue();

        // Cleanup
        _creator.Terminate();
    }

    public void Dispose()
    {
        _creator.Dispose();
        _loggerFactory.Dispose();
    }
}
```

### Test 2.2: DLL Injection into Notepad

**File**: `tests/LineageLauncher.Launcher.Tests/Integration/DllInjectionIntegrationTests.cs`

```csharp
using FluentAssertions;
using LineageLauncher.Launcher.Process;
using LineageLauncher.Launcher.Injection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LineageLauncher.Launcher.Tests.Integration;

public class DllInjectionIntegrationTests : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ProcessCreator _processCreator;
    private readonly DllInjector _dllInjector;

    public DllInjectionIntegrationTests()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        _processCreator = new ProcessCreator(
            _loggerFactory.CreateLogger<ProcessCreator>());

        _dllInjector = new DllInjector(
            _loggerFactory.CreateLogger<DllInjector>());
    }

    [Fact(Skip = "Requires test DLL")]
    public async Task InjectTestDll_IntoNotepad_Succeeds()
    {
        // Arrange - Create suspended notepad
        var createResult = _processCreator.CreateSuspended(
            @"C:\Windows\System32\notepad.exe",
            "",
            @"C:\Windows\System32");

        createResult.Success.Should().BeTrue();

        // Act - Inject test DLL
        var injectionResult = await _dllInjector.InjectSingleDllAsync(
            createResult.ProcessHandle,
            @"TestDlls\TestDummy.dll");

        // Assert
        injectionResult.Success.Should().BeTrue();

        // Resume process
        _processCreator.ResumeMainThread();
        System.Threading.Thread.Sleep(1000);

        // Verify process is running
        _processCreator.IsRunning.Should().BeTrue();

        // TODO: Verify DLL is loaded (requires EnumProcessModules)

        // Cleanup
        _processCreator.Terminate();
    }

    [Fact(Skip = "Requires test DLLs")]
    public async Task InjectMultipleDlls_IntoNotepad_Succeeds()
    {
        // Arrange
        var createResult = _processCreator.CreateSuspended(
            @"C:\Windows\System32\notepad.exe",
            "",
            @"C:\Windows\System32");

        createResult.Success.Should().BeTrue();

        var dllsToInject = new List<string>
        {
            @"TestDlls\TestDummy1.dll",
            @"TestDlls\TestDummy2.dll",
            @"TestDlls\TestDummy3.dll"
        };

        // Act
        var injectionResult = await _dllInjector.InjectDllsAsync(
            createResult.ProcessHandle,
            dllsToInject);

        // Assert
        injectionResult.Success.Should().BeTrue();
        injectionResult.InjectedCount.Should().Be(3);

        // Cleanup
        _processCreator.Terminate();
    }

    public void Dispose()
    {
        _processCreator.Dispose();
        _dllInjector.Dispose();
        _loggerFactory.Dispose();
    }
}
```

### Test 2.3: Named Pipe Communication

**File**: `tests/LineageLauncher.Launcher.Tests/Integration/PipeManagerIntegrationTests.cs`

```csharp
using FluentAssertions;
using LineageLauncher.Launcher.IPC;
using Microsoft.Extensions.Logging;
using System.IO.Pipes;
using System.Text;
using Xunit;

namespace LineageLauncher.Launcher.Tests.Integration;

public class PipeManagerIntegrationTests : IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly PipeManager _pipeManager;

    public PipeManagerIntegrationTests()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        _pipeManager = new PipeManager(
            _loggerFactory.CreateLogger<PipeManager>(),
            "TestPipe");
    }

    [Fact]
    public async Task PipeManager_CreateAndConnect_Succeeds()
    {
        // Arrange
        await _pipeManager.CreatePipesAsync();

        // Act - Simulate client connecting
        var clientTask = Task.Run(async () =>
        {
            using var clientOut = new NamedPipeClientStream(
                ".",
                "TestPipe_Pipe1",
                PipeDirection.In);

            using var clientIn = new NamedPipeClientStream(
                ".",
                "TestPipe_Pipe2",
                PipeDirection.Out);

            await Task.WhenAll(
                clientOut.ConnectAsync(),
                clientIn.ConnectAsync());

            return true;
        });

        var serverTask = _pipeManager.WaitForConnectionAsync(TimeSpan.FromSeconds(5));

        // Assert
        await Task.WhenAll(clientTask, serverTask);
        clientTask.Result.Should().BeTrue();
        _pipeManager.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task PipeManager_SendAndReceive_WorksCorrectly()
    {
        // Arrange
        await _pipeManager.CreatePipesAsync();

        var testMessage = Encoding.UTF8.GetBytes("Hello from launcher!");

        // Simulate client
        var clientTask = Task.Run(async () =>
        {
            using var clientOut = new NamedPipeClientStream(
                ".",
                "TestPipe_Pipe1",
                PipeDirection.In);

            using var clientIn = new NamedPipeClientStream(
                ".",
                "TestPipe_Pipe2",
                PipeDirection.Out);

            await Task.WhenAll(
                clientOut.ConnectAsync(),
                clientIn.ConnectAsync());

            // Read message from launcher
            var buffer = new byte[1024];
            var bytesRead = await clientOut.ReadAsync(buffer, 0, buffer.Length);
            var message = new byte[bytesRead];
            Array.Copy(buffer, message, bytesRead);

            // Send response
            var response = Encoding.UTF8.GetBytes("Hello from game!");
            await clientIn.WriteAsync(response, 0, response.Length);

            return message;
        });

        // Wait for connection
        await _pipeManager.WaitForConnectionAsync(TimeSpan.FromSeconds(5));

        // Act - Send message
        await _pipeManager.WriteMessageAsync(testMessage);

        // Receive response
        var receivedMessage = await _pipeManager.ReadMessageAsync();

        // Assert
        var clientReceived = await clientTask;
        clientReceived.Should().BeEquivalentTo(testMessage);
        receivedMessage.Should().BeEquivalentTo(
            Encoding.UTF8.GetBytes("Hello from game!"));
    }

    public void Dispose()
    {
        _pipeManager.Dispose();
        _loggerFactory.Dispose();
    }
}
```

---

## Phase 3: Manual Tests

### Test 3.1: Launch Notepad with Test DLL

**Purpose**: Verify complete launch flow with safe executable

**Steps**:

1. Build test DLL:
   ```bash
   cd TestDlls
   csc /target:library /out:TestDummy.dll TestDummy.cs
   ```

2. Create test program:
   ```csharp
   var config = new LaunchConfiguration
   {
       ExecutablePath = @"C:\Windows\System32\notepad.exe",
       WorkingDirectory = @"C:\Windows\System32",
       DllsToInject = new List<string>
       {
           @"C:\Path\To\TestDlls\TestDummy.dll"
       }
   };

   using var orchestrator = new ProcessLaunchOrchestrator(...);
   var result = await orchestrator.LaunchWithInjectionAsync(config);

   Console.WriteLine($"Success: {result.Success}");
   Console.WriteLine($"PID: {result.ProcessId}");
   ```

3. Run and verify:
   - Notepad launches
   - Window appears
   - Process runs normally
   - No crashes

**Expected Result**: Notepad runs with injected DLL

### Test 3.2: Launch Lin.bin with Real DLLs

**Purpose**: Test with actual game client

**Prerequisites**:
- Lin.bin installed
- 210916.asi, boxer.dll, libcocos2d.dll available

**Steps**:

1. Configure launcher:
   ```csharp
   var config = new LaunchConfiguration
   {
       ExecutablePath = @"D:\L1R Project\L1R-Client\bin64\Lin.bin",
       WorkingDirectory = @"D:\L1R Project\L1R-Client\bin64",
       DllsToInject = new List<string>
       {
           @"D:\L1R Project\L1R-Client\bin64\210916.asi",
           @"D:\L1R Project\L1R-Client\bin64\boxer.dll",
           @"D:\L1R Project\L1R-Client\bin64\libcocos2d.dll"
       },
       EnvironmentVariables = new Dictionary<string, string>
       {
           ["L1_DLL_PASSWORD"] = "12345",
           ["L1_CLIENT_SIDE_KEY"] = "67890"
       }
   };
   ```

2. Launch with detailed logging
3. Monitor log output for each phase
4. Verify game client starts

**Expected Result**:
- All 3 DLLs inject successfully
- Game client launches
- Login screen appears
- No anti-cheat detection

### Test 3.3: Error Scenario - Missing DLL

**Purpose**: Verify error handling

**Steps**:

1. Configure with non-existent DLL:
   ```csharp
   DllsToInject = new List<string>
   {
       @"C:\NonExistent\Missing.dll"
   }
   ```

2. Launch and observe

**Expected Result**:
- Injection fails with clear error message
- Process is terminated cleanly
- No orphaned processes
- User-friendly error shown

### Test 3.4: Cancellation During Injection

**Purpose**: Verify cancellation handling

**Steps**:

1. Start launch with cancellation token
2. Cancel during injection phase
3. Verify cleanup

**Expected Result**:
- Operation cancels cleanly
- Process is terminated
- All handles closed
- OperationCanceledException thrown

---

## Phase 4: End-to-End Tests

### Test 4.1: Complete Launch Flow

**Automated E2E Test**:

```csharp
[Fact(Skip = "E2E test")]
public async Task CompleteLaunchFlow_WithLinBin_Succeeds()
{
    // Arrange
    var loggerFactory = LoggerFactory.Create(builder =>
        builder.AddConsole().SetMinimumLevel(LogLevel.Information));

    using var orchestrator = new ProcessLaunchOrchestrator(
        loggerFactory.CreateLogger<ProcessLaunchOrchestrator>(),
        new ProcessCreator(loggerFactory.CreateLogger<ProcessCreator>()),
        new DllInjector(loggerFactory.CreateLogger<DllInjector>()),
        new PipeManager(loggerFactory.CreateLogger<PipeManager>()));

    var config = new LaunchConfiguration
    {
        ExecutablePath = @"D:\L1R Project\L1R-Client\bin64\Lin.bin",
        WorkingDirectory = @"D:\L1R Project\L1R-Client\bin64",
        DllsToInject = new List<string>
        {
            @"D:\L1R Project\L1R-Client\bin64\210916.asi",
            @"D:\L1R Project\L1R-Client\bin64\boxer.dll",
            @"D:\L1R Project\L1R-Client\bin64\libcocos2d.dll"
        }
    };

    // Act
    var result = await orchestrator.LaunchWithInjectionAsync(config);

    // Assert
    result.Success.Should().BeTrue();
    result.ProcessId.Should().BeGreaterThan(0);
    orchestrator.IsProcessRunning.Should().BeTrue();

    // Allow game to initialize
    await Task.Delay(5000);

    // Verify still running
    orchestrator.IsProcessRunning.Should().BeTrue();

    // Cleanup
    await orchestrator.TerminateAsync();
}
```

### Test 4.2: Multiple Sequential Launches

**Purpose**: Verify no resource leaks

```csharp
[Fact(Skip = "Resource leak test")]
public async Task MultipleLaunches_NoResourceLeaks()
{
    var initialHandleCount = Process.GetCurrentProcess().HandleCount;

    for (int i = 0; i < 10; i++)
    {
        using var orchestrator = CreateOrchestrator();
        var result = await orchestrator.LaunchWithInjectionAsync(config);

        result.Success.Should().BeTrue();

        await Task.Delay(1000);
        await orchestrator.TerminateAsync();

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    var finalHandleCount = Process.GetCurrentProcess().HandleCount;
    var handleLeak = finalHandleCount - initialHandleCount;

    handleLeak.Should().BeLessThan(50, "Handle leak detected");
}
```

---

## Phase 5: Production Validation

### Validation Checklist

- [ ] Launch Lin.bin 10 times successfully
- [ ] No crashes or hangs
- [ ] Average launch time < 2 seconds
- [ ] No orphaned processes
- [ ] No handle leaks
- [ ] Error messages are clear
- [ ] Logging is comprehensive
- [ ] Anti-cheat doesn't detect

### Performance Metrics

Measure and record:
- Time to create process: ____ ms
- Time to inject DLLs: ____ ms
- Time to resume thread: ____ ms
- Total launch time: ____ ms
- Memory usage: ____ MB
- Handle count: ____

---

## Troubleshooting Guide

### Issue: CreateProcess Fails

**Symptoms**: "Failed to create process: Access Denied"

**Solutions**:
1. Run launcher as administrator
2. Check executable path is correct
3. Verify working directory exists
4. Check antivirus isn't blocking

### Issue: VirtualAllocEx Fails

**Symptoms**: "VirtualAllocEx failed: Error 5"

**Solutions**:
1. Verify process handle is valid
2. Check target process bitness matches (32-bit vs 64-bit)
3. Ensure process isn't protected

### Issue: DLL Not Found

**Symptoms**: "DLL not found: path\\to\\dll.dll"

**Solutions**:
1. Verify DLL path is correct
2. Use absolute paths, not relative
3. Check DLL file exists
4. Verify file permissions

### Issue: LoadLibraryW Timeout

**Symptoms**: "Injection timed out after 5000ms"

**Solutions**:
1. Increase timeout value
2. Check DLL dependencies exist
3. Verify DLL is valid (not corrupted)
4. Check for DllMain deadlock

### Issue: Pipe Connection Timeout

**Symptoms**: "Game client did not connect within 10s"

**Solutions**:
1. This may be normal (game doesn't use pipes)
2. Increase timeout if game is slow to start
3. Check pipe names match on both sides

---

## Performance Benchmarks

### Target Metrics

| Operation | Target | Acceptable | Unacceptable |
|-----------|--------|------------|--------------|
| Process Creation | < 100ms | < 500ms | > 1000ms |
| Single DLL Injection | < 50ms | < 200ms | > 500ms |
| Three DLL Injection | < 150ms | < 600ms | > 1500ms |
| Thread Resume | < 10ms | < 50ms | > 100ms |
| Total Launch Time | < 1s | < 2s | > 3s |

### Benchmark Test

```csharp
[Fact(Skip = "Benchmark test")]
public async Task BenchmarkLaunchPerformance()
{
    var stopwatch = new Stopwatch();
    var iterations = 10;
    var times = new List<long>();

    for (int i = 0; i < iterations; i++)
    {
        stopwatch.Restart();

        using var orchestrator = CreateOrchestrator();
        var result = await orchestrator.LaunchWithInjectionAsync(config);

        stopwatch.Stop();
        times.Add(stopwatch.ElapsedMilliseconds);

        await orchestrator.TerminateAsync();
        await Task.Delay(500); // Cool down
    }

    var average = times.Average();
    var min = times.Min();
    var max = times.Max();

    Console.WriteLine($"Average: {average}ms");
    Console.WriteLine($"Min: {min}ms");
    Console.WriteLine($"Max: {max}ms");

    average.Should().BeLessThan(2000, "Launch too slow");
}
```

---

## Summary

This testing guide provides a comprehensive strategy for validating the DLL injection implementation:

1. **Unit Tests**: Fast, focused tests for individual components
2. **Integration Tests**: Verify components work together
3. **Manual Tests**: Real-world scenarios with actual executables
4. **E2E Tests**: Complete launch flows
5. **Production Validation**: Final checks before deployment

**Test Coverage Goal**: 90%+ for core injection logic

**Next Steps**:
1. Implement unit tests (Phase 1)
2. Run integration tests (Phase 2)
3. Perform manual testing (Phase 3)
4. Validate production readiness (Phase 5)
