# DLL Injection Implementation Roadmap
## Step-by-Step Guide to Implementation

**Date**: 2025-11-12
**Target Completion**: 11 days
**Status**: Ready to Begin

---

## Executive Summary

This roadmap provides a day-by-day plan for implementing DLL injection in the Lineage custom launcher. Follow this guide sequentially to ensure a smooth implementation.

### Overview

- **Total Days**: 11
- **New Files**: 7
- **Modified Files**: 1
- **Total Code**: ~2,000 lines
- **Test Coverage**: 90%+

---

## Quick Start Checklist

Before you begin:

- [ ] Read `DLL_INJECTION_REQUIREMENTS.md`
- [ ] Review `DLL_INJECTION_ARCHITECTURE.md`
- [ ] Study `DLL_INJECTION_CLASS_TEMPLATES.md`
- [ ] Set up test environment
- [ ] Create feature branch: `git checkout -b feature/dll-injection`
- [ ] Install required tools (Visual Studio 2022, .NET 8.0 SDK)

---

## Day 1: Foundation Setup

### Goals
- Set up project structure
- Implement native interop
- Basic structure marshalling

### Tasks

#### 1.1 Create Directory Structure
```bash
cd src/LineageLauncher.Launcher
mkdir Native
mkdir Process
mkdir Injection
mkdir IPC
mkdir Orchestration
```

#### 1.2 Implement NativeInterop.cs
**Location**: `src/LineageLauncher.Launcher/Native/NativeInterop.cs`

**Steps**:
1. Copy template from `DLL_INJECTION_CLASS_TEMPLATES.md`
2. Implement all P/Invoke declarations
3. Add error handling wrappers
4. Test compilation

**Time Estimate**: 3 hours

#### 1.3 Implement NativeStructures.cs
**Location**: `src/LineageLauncher.Launcher/Native/NativeStructures.cs`

**Steps**:
1. Copy template from class templates document
2. Define all structures with correct layout
3. Define all enums
4. Verify sizes with Marshal.SizeOf

**Time Estimate**: 2 hours

#### 1.4 Create Unit Tests
**Location**: `tests/LineageLauncher.Launcher.Tests/Native/`

**Files**:
- `NativeInteropTests.cs`
- `NativeStructuresTests.cs`

**Steps**:
1. Test structure marshalling
2. Test GetModuleHandle
3. Test GetProcAddress
4. Run all tests: `dotnet test`

**Time Estimate**: 2 hours

### Deliverables
- ✅ NativeInterop.cs compiles
- ✅ NativeStructures.cs compiles
- ✅ All unit tests pass
- ✅ No compiler warnings

### Validation
```bash
cd src/LineageLauncher.Launcher
dotnet build
cd ../../tests/LineageLauncher.Launcher.Tests
dotnet test --filter "FullyQualifiedName~Native"
```

---

## Day 2: Process Creation

### Goals
- Implement ProcessCreator
- Test process creation in suspended state
- Verify thread resumption works

### Tasks

#### 2.1 Implement ProcessCreator.cs
**Location**: `src/LineageLauncher.Launcher/Process/ProcessCreator.cs`

**Steps**:
1. Copy template from class templates document
2. Implement CreateSuspended method
3. Implement ResumeMainThread method
4. Implement SuspendMainThread method
5. Implement Terminate method
6. Implement Dispose pattern

**Time Estimate**: 4 hours

#### 2.2 Implement ProcessCreationResult.cs
**Location**: `src/LineageLauncher.Launcher/Process/ProcessCreationResult.cs`

**Steps**:
1. Define result class
2. Add Success/Failed factory methods

**Time Estimate**: 30 minutes

#### 2.3 Create Unit Tests
**Location**: `tests/LineageLauncher.Launcher.Tests/Process/ProcessCreatorTests.cs`

**Tests**:
- Invalid path returns failure
- Empty path returns failure
- Resume without creation throws exception
- Dispose is idempotent

**Time Estimate**: 2 hours

#### 2.4 Manual Testing
**Test**: Create suspended notepad.exe

```csharp
using var creator = new ProcessCreator(logger);
var result = creator.CreateSuspended(
    @"C:\Windows\System32\notepad.exe",
    "",
    @"C:\Windows\System32");

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"PID: {result.ProcessId}");

Thread.Sleep(2000); // Process should be suspended

creator.ResumeMainThread(); // Window should appear

Thread.Sleep(2000);
creator.Terminate();
```

**Time Estimate**: 1 hour

### Deliverables
- ✅ ProcessCreator.cs compiles
- ✅ Can create suspended process
- ✅ Can resume suspended process
- ✅ Can terminate process
- ✅ All unit tests pass

### Validation
```bash
dotnet test --filter "FullyQualifiedName~Process"
```

---

## Day 3: DLL Injection - Part 1

### Goals
- Implement memory allocation
- Implement memory writing
- Implement remote thread creation

### Tasks

#### 3.1 Implement DllInjector.cs (Core Methods)
**Location**: `src/LineageLauncher.Launcher/Injection/DllInjector.cs`

**Focus Today**:
- Memory allocation (VirtualAllocEx)
- Memory writing (WriteProcessMemory)
- Remote thread creation (CreateRemoteThread)
- Thread waiting (WaitForSingleObject)
- Memory cleanup (VirtualFreeEx)

**Steps**:
1. Implement InjectSingleDllAsync skeleton
2. Implement memory allocation logic
3. Implement memory write logic
4. Test with dummy data (not actual DLL yet)

**Time Estimate**: 5 hours

#### 3.2 Create Unit Tests
**Location**: `tests/LineageLauncher.Launcher.Tests/Injection/DllInjectorTests.cs`

**Tests**:
- Empty DLL list returns failure
- Non-existent DLL returns failure
- Invalid process handle returns failure

**Time Estimate**: 2 hours

### Deliverables
- ✅ Memory allocation works
- ✅ Memory writing works
- ✅ Remote thread creation works
- ✅ Unit tests pass

---

## Day 4: DLL Injection - Part 2

### Goals
- Complete DLL injection logic
- Test with real DLL
- Verify LoadLibraryW call

### Tasks

#### 4.1 Complete DllInjector.cs
**Steps**:
1. Implement LoadLibraryW address lookup
2. Implement DLL path encoding (Unicode)
3. Complete InjectSingleDllAsync
4. Implement InjectDllsAsync (multiple DLLs)
5. Add error handling and logging

**Time Estimate**: 4 hours

#### 4.2 Implement DllInjectionResult.cs
**Location**: `src/LineageLauncher.Launcher/Injection/DllInjectionResult.cs`

**Time Estimate**: 30 minutes

#### 4.3 Create Test DLL
**Location**: `tests/TestDlls/TestDummy.dll`

```csharp
// TestDummy.cs
using System;
using System.Windows.Forms;

namespace TestDummy
{
    public class Dummy
    {
        [DllExport]
        public static void Init()
        {
            MessageBox.Show("DLL Injected!", "TestDummy");
        }
    }
}
```

Compile: `csc /target:library /out:TestDummy.dll TestDummy.cs`

**Time Estimate**: 1 hour

#### 4.4 Integration Testing
**Test**: Inject test DLL into notepad

```csharp
using var creator = new ProcessCreator(logger);
using var injector = new DllInjector(logger);

var createResult = creator.CreateSuspended(
    @"C:\Windows\System32\notepad.exe",
    "",
    @"C:\Windows\System32");

var injectResult = await injector.InjectSingleDllAsync(
    createResult.ProcessHandle,
    @"TestDlls\TestDummy.dll");

Console.WriteLine($"Injection success: {injectResult.Success}");

creator.ResumeMainThread();
// Notepad should show message box
```

**Time Estimate**: 2 hours

### Deliverables
- ✅ Can inject DLL into suspended process
- ✅ LoadLibraryW executes correctly
- ✅ Test DLL loads successfully
- ✅ Integration tests pass

### Validation
```bash
dotnet test --filter "FullyQualifiedName~Injection"
```

---

## Day 5: Named Pipes - Part 1

### Goals
- Implement pipe creation
- Test basic pipe operations
- Implement connection waiting

### Tasks

#### 5.1 Implement PipeManager.cs (Creation)
**Location**: `src/LineageLauncher.Launcher/IPC/PipeManager.cs`

**Focus Today**:
- CreatePipesAsync
- WaitForConnectionAsync
- IsConnected property
- Dispose pattern

**Steps**:
1. Implement pipe creation (both directions)
2. Implement connection waiting with timeout
3. Add logging

**Time Estimate**: 4 hours

#### 5.2 Create Unit Tests
**Location**: `tests/LineageLauncher.Launcher.Tests/IPC/PipeManagerTests.cs`

**Tests**:
- CreatePipes succeeds
- CreatePipes twice throws exception
- WaitForConnection without creation throws
- Dispose is idempotent

**Time Estimate**: 2 hours

#### 5.3 Manual Testing
**Test**: Create and connect to pipes

Create test client:
```csharp
// TestPipeClient.cs
using var client1 = new NamedPipeClientStream(
    ".",
    "LineageLauncher_Pipe1",
    PipeDirection.In);

using var client2 = new NamedPipeClientStream(
    ".",
    "LineageLauncher_Pipe2",
    PipeDirection.Out);

await Task.WhenAll(
    client1.ConnectAsync(),
    client2.ConnectAsync());

Console.WriteLine("Connected!");
```

**Time Estimate**: 1 hour

### Deliverables
- ✅ Can create named pipes
- ✅ Can wait for connections
- ✅ Timeout works correctly
- ✅ Unit tests pass

---

## Day 6: Named Pipes - Part 2

### Goals
- Implement message sending/receiving
- Test bidirectional communication
- Handle disconnections

### Tasks

#### 6.1 Complete PipeManager.cs
**Steps**:
1. Implement ReadMessageAsync
2. Implement WriteMessageAsync
3. Add error handling for disconnections
4. Test bidirectional communication

**Time Estimate**: 3 hours

#### 6.2 Integration Testing
**Test**: Full bidirectional communication

Server (PipeManager):
```csharp
using var pipeManager = new PipeManager(logger);
await pipeManager.CreatePipesAsync();

var connectTask = pipeManager.WaitForConnectionAsync(TimeSpan.FromSeconds(10));
// Start client in separate process/thread
await connectTask;

await pipeManager.WriteMessageAsync(Encoding.UTF8.GetBytes("Hello from launcher"));
var response = await pipeManager.ReadMessageAsync();
Console.WriteLine($"Received: {Encoding.UTF8.GetString(response)}");
```

Client:
```csharp
// Connect and communicate
```

**Time Estimate**: 3 hours

#### 6.3 Create Integration Tests
**Location**: `tests/LineageLauncher.Launcher.Tests/Integration/PipeManagerIntegrationTests.cs`

**Time Estimate**: 2 hours

### Deliverables
- ✅ Can send messages
- ✅ Can receive messages
- ✅ Bidirectional communication works
- ✅ Integration tests pass

### Validation
```bash
dotnet test --filter "FullyQualifiedName~IPC"
```

---

## Day 7: Orchestration - Part 1

### Goals
- Implement orchestrator skeleton
- Coordinate components
- Implement launch phases

### Tasks

#### 7.1 Implement ProcessLaunchOrchestrator.cs (Structure)
**Location**: `src/LineageLauncher.Launcher/Orchestration/ProcessLaunchOrchestrator.cs`

**Focus Today**:
- Class structure
- Dependency injection
- LaunchWithInjectionAsync skeleton
- Phase enum

**Steps**:
1. Set up class with all dependencies
2. Implement constructor
3. Define launch phases
4. Implement basic error handling structure

**Time Estimate**: 3 hours

#### 7.2 Implement LaunchConfiguration.cs
**Location**: `src/LineageLauncher.Launcher/Orchestration/LaunchConfiguration.cs`

**Steps**:
1. Define all configuration properties
2. Add validation logic
3. Add sensible defaults

**Time Estimate**: 1 hour

#### 7.3 Implement LaunchResult.cs
**Location**: `src/LineageLauncher.Launcher/Orchestration/LaunchResult.cs`

**Steps**:
1. Define result properties
2. Add factory methods
3. Add user-friendly error messages

**Time Estimate**: 1 hour

### Deliverables
- ✅ Orchestrator structure complete
- ✅ Configuration class ready
- ✅ Result class ready
- ✅ Compiles without errors

---

## Day 8: Orchestration - Part 2

### Goals
- Implement complete launch flow
- Add progress reporting
- Implement rollback logic

### Tasks

#### 8.1 Complete LaunchWithInjectionAsync
**Steps**:
1. Implement Phase 1: Pipe creation
2. Implement Phase 2: Process creation
3. Implement Phase 3: DLL injection
4. Implement Phase 4: Thread resume
5. Implement Phase 5: Pipe connection
6. Add progress reporting
7. Add rollback on failure

**Time Estimate**: 5 hours

#### 8.2 Implement TerminateAsync
**Steps**:
1. Terminate process
2. Cleanup resources
3. Add logging

**Time Estimate**: 1 hour

#### 8.3 Integration Testing
**Test**: Complete launch flow with notepad + test DLL

```csharp
var config = new LaunchConfiguration
{
    ExecutablePath = @"C:\Windows\System32\notepad.exe",
    WorkingDirectory = @"C:\Windows\System32",
    DllsToInject = new List<string>
    {
        @"TestDlls\TestDummy.dll"
    }
};

using var orchestrator = new ProcessLaunchOrchestrator(
    logger,
    new ProcessCreator(logger),
    new DllInjector(logger),
    new PipeManager(logger));

var progress = new Progress<string>(msg => Console.WriteLine(msg));
var result = await orchestrator.LaunchWithInjectionAsync(config, progress);

Console.WriteLine($"Success: {result.Success}");
Console.WriteLine($"PID: {result.ProcessId}");
```

**Time Estimate**: 2 hours

### Deliverables
- ✅ Complete launch flow works
- ✅ Progress reporting works
- ✅ Rollback on failure works
- ✅ All components integrated

---

## Day 9: LinBinLauncher Integration

### Goals
- Integrate orchestrator into LinBinLauncher
- Replace Process.Start() with injection flow
- Test with real Lin.bin

### Tasks

#### 9.1 Modify LinBinLauncher.cs
**Location**: `src/LineageLauncher.Launcher/LinBinLauncher.cs`

**Changes**:
1. Add orchestrator dependencies
2. Replace `LaunchGameInternalAsync` implementation
3. Update DLL paths configuration
4. Add progress reporting
5. Update error handling

**Before** (lines 96-122):
```csharp
var startInfo = new ProcessStartInfo { ... };
_gameProcess = Process.Start(startInfo);
```

**After**:
```csharp
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
    }
};

using var orchestrator = new ProcessLaunchOrchestrator(...);
var result = await orchestrator.LaunchWithInjectionAsync(config);
```

**Time Estimate**: 3 hours

#### 9.2 Update Dependency Injection
**Location**: Program.cs or startup configuration

**Add Services**:
```csharp
services.AddTransient<ProcessCreator>();
services.AddTransient<DllInjector>();
services.AddTransient<PipeManager>();
services.AddTransient<ProcessLaunchOrchestrator>();
```

**Time Estimate**: 1 hour

#### 9.3 Testing with Lin.bin
**Prerequisites**:
- Lin.bin installed
- DLLs present: 210916.asi, boxer.dll, libcocos2d.dll
- Server configuration in appsettings.json

**Test Scenarios**:
1. Normal launch
2. Missing DLL
3. Invalid path
4. Cancellation

**Time Estimate**: 3 hours

### Deliverables
- ✅ LinBinLauncher uses orchestrator
- ✅ Lin.bin launches successfully
- ✅ All DLLs inject correctly
- ✅ Game runs normally

### Validation
```bash
# Build launcher
cd src/LineageLauncher.App
dotnet build

# Run launcher
dotnet run

# Launch Lin.bin and verify
# - Game starts
# - Login screen appears
# - No crashes
```

---

## Day 10: Testing and Bug Fixes

### Goals
- Comprehensive testing
- Fix discovered bugs
- Optimize performance

### Tasks

#### 10.1 Run Full Test Suite
```bash
cd tests/LineageLauncher.Launcher.Tests
dotnet test --collect:"XPlat Code Coverage"
```

**Check**:
- All unit tests pass
- All integration tests pass
- Code coverage > 90%

**Time Estimate**: 2 hours

#### 10.2 Manual Testing
**Test Scenarios**:
1. ✅ Launch Lin.bin 5 times consecutively
2. ✅ Launch, play for 5 minutes, exit
3. ✅ Launch with invalid DLL path
4. ✅ Launch with missing DLL file
5. ✅ Cancel during injection phase
6. ✅ Multiple launcher instances

**Time Estimate**: 3 hours

#### 10.3 Performance Testing
**Measure**:
- Process creation time
- DLL injection time (per DLL)
- Total launch time
- Memory usage
- Handle count

**Target**: Total launch time < 2 seconds

**Time Estimate**: 2 hours

#### 10.4 Bug Fixes
Fix any issues discovered during testing

**Time Estimate**: 2 hours (buffer)

### Deliverables
- ✅ All tests pass
- ✅ No known bugs
- ✅ Performance meets targets
- ✅ Code coverage > 90%

---

## Day 11: Documentation and Release

### Goals
- Complete documentation
- Create release notes
- Prepare for deployment

### Tasks

#### 11.1 Code Documentation
**Review**:
- XML comments on all public methods
- README.md updated
- Architecture diagrams current

**Time Estimate**: 2 hours

#### 11.2 Create Release Notes
**File**: `CHANGELOG.md`

```markdown
## [2.0.0] - 2025-XX-XX

### Added
- DLL injection support for Lin.bin launch
- Named pipe IPC for launcher-game communication
- Process orchestration for complete launch flow
- Comprehensive error handling and rollback
- Detailed logging for all injection phases

### Changed
- Replaced Process.Start() with DLL injection flow
- Updated launch process to inject required DLLs

### Fixed
- Lin.bin now launches correctly with DLL dependencies
```

**Time Estimate**: 1 hour

#### 11.3 User Documentation
**File**: `docs/USER_GUIDE.md`

**Topics**:
- How to use the launcher
- Troubleshooting common issues
- Error message meanings
- Configuration options

**Time Estimate**: 2 hours

#### 11.4 Deployment Preparation
**Checklist**:
- [ ] Build Release configuration
- [ ] Run all tests in Release mode
- [ ] Create installer/package
- [ ] Test installation on clean machine
- [ ] Prepare rollback plan

**Time Estimate**: 3 hours

### Deliverables
- ✅ Documentation complete
- ✅ Release notes written
- ✅ Deployment package ready
- ✅ Rollback plan documented

---

## Post-Implementation Checklist

After all 11 days:

### Code Quality
- [ ] All files have XML documentation
- [ ] No compiler warnings
- [ ] Code follows C# conventions
- [ ] SOLID principles followed
- [ ] DRY principle followed

### Testing
- [ ] 90%+ code coverage
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Manual testing complete
- [ ] Performance benchmarks met

### Documentation
- [ ] Architecture document current
- [ ] API documentation complete
- [ ] User guide written
- [ ] Troubleshooting guide complete
- [ ] Release notes written

### Deployment
- [ ] Builds in Release mode
- [ ] All tests pass in Release
- [ ] Installer tested
- [ ] Rollback plan ready
- [ ] Stakeholders informed

---

## Risk Mitigation

### High-Risk Areas

1. **Anti-Cheat Detection**
   - **Risk**: Game detects injection as cheat
   - **Mitigation**: Inject before anti-cheat loads, use official DLLs only
   - **Fallback**: Disable injection, use original launcher

2. **Process Handle Leaks**
   - **Risk**: Handles not closed, resource exhaustion
   - **Mitigation**: IDisposable pattern, comprehensive testing
   - **Monitoring**: Track handle count in tests

3. **DLL Load Failures**
   - **Risk**: DLL doesn't load, game crashes
   - **Mitigation**: Validate DLLs before injection, rollback on failure
   - **Logging**: Detailed error messages

4. **Client Version Changes**
   - **Risk**: New client version breaks injection
   - **Mitigation**: Version detection, fallback to original launcher
   - **Testing**: Test with multiple client versions

---

## Success Criteria

### Must Have
- ✅ Lin.bin launches successfully
- ✅ All 3 DLLs inject correctly
- ✅ Game runs without crashes
- ✅ Launch time < 2 seconds
- ✅ No resource leaks

### Should Have
- ✅ Named pipe communication works
- ✅ Progress reporting works
- ✅ Error messages are clear
- ✅ Logging is comprehensive

### Nice to Have
- ✅ Multiple client version support
- ✅ Automatic rollback on failure
- ✅ Performance metrics logging

---

## Daily Standup Template

Use this template for daily progress tracking:

```
Date: YYYY-MM-DD
Day: X of 11

✅ Completed:
- Task 1
- Task 2

🚧 In Progress:
- Task 3

⏰ Blocked:
- Issue 1 (reason)

📊 Metrics:
- Tests passing: X/Y
- Code coverage: Z%
- Known bugs: N

🎯 Tomorrow:
- Task 4
- Task 5
```

---

## Emergency Contacts

If you encounter issues:

1. **Architecture Questions**: Review `DLL_INJECTION_ARCHITECTURE.md`
2. **Implementation Help**: Check `DLL_INJECTION_CLASS_TEMPLATES.md`
3. **Testing Issues**: See `DLL_INJECTION_TESTING_GUIDE.md`
4. **Windows API**: Microsoft Docs (https://docs.microsoft.com/windows/win32/)

---

## Conclusion

This roadmap provides a complete, day-by-day guide to implementing DLL injection. Follow it sequentially, test thoroughly, and document everything.

**Remember**: This is a legitimate feature required by the game client architecture, not a cheat or hack.

**Good luck with implementation!** 🚀

---

**Document Status**: Ready for Implementation
**Last Updated**: 2025-11-12
**Version**: 1.0
