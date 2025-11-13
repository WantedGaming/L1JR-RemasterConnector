# DLL Injection Complete Guide
## Master Document for Lineage Launcher Implementation

**Date**: 2025-11-12
**Status**: Architecture Complete - Ready for Implementation
**Project**: L1R Custom Launcher

---

## Document Overview

This master document provides links and summaries of all DLL injection architecture documentation.

### Documentation Set

1. **Requirements** - `DLL_INJECTION_REQUIREMENTS.md`
2. **Architecture** - `DLL_INJECTION_ARCHITECTURE.md`
3. **Class Templates** - `DLL_INJECTION_CLASS_TEMPLATES.md`
4. **Testing Guide** - `DLL_INJECTION_TESTING_GUIDE.md`
5. **Implementation Roadmap** - `DLL_INJECTION_IMPLEMENTATION_ROADMAP.md`
6. **This Document** - `DLL_INJECTION_COMPLETE_GUIDE.md`

---

## Quick Navigation

### For Project Managers
- [Executive Summary](#executive-summary)
- [Timeline and Deliverables](#timeline-and-deliverables)
- [Risk Assessment](#risk-assessment)
- [Success Metrics](#success-metrics)

### For Architects
- [Architecture Overview](#architecture-overview)
- [Component Design](#component-design)
- [Integration Points](#integration-points)
- [Security Considerations](#security-considerations)

### For Developers
- [Getting Started](#getting-started)
- [Implementation Order](#implementation-order)
- [Code Templates](#code-templates)
- [Testing Strategy](#testing-strategy)

### For QA
- [Test Plan](#test-plan)
- [Test Cases](#test-cases)
- [Validation Criteria](#validation-criteria)

---

## Executive Summary

### The Problem

Lin.bin (Lineage Remaster client) requires specific DLLs (210916.asi, boxer.dll, libcocos2d.dll) to be injected into its process space before execution. Simple `Process.Start()` does not work because the client expects these DLLs to be loaded at startup.

### The Solution

Implement a complete DLL injection architecture that:
1. Creates Lin.bin in SUSPENDED state
2. Injects required DLLs using Windows API
3. Resumes execution after injection
4. Establishes IPC communication via named pipes

### Technical Approach

- **Language**: C# (.NET 8.0)
- **Platform**: Windows only
- **APIs**: Win32 API via P/Invoke
- **Injection Method**: LoadLibraryW via CreateRemoteThread
- **IPC**: Named Pipes (NamedPipeServerStream)
- **Pattern**: Orchestrator pattern for coordination

### Key Statistics

| Metric | Value |
|--------|-------|
| New Files | 7 |
| Modified Files | 1 |
| Total New Code | ~2,000 lines |
| Implementation Time | 11 days |
| Test Coverage Target | 90%+ |
| Launch Time Target | < 2 seconds |

---

## Timeline and Deliverables

### Phase 1: Foundation (Days 1-2)
**Deliverables**:
- NativeInterop.cs (Win32 API declarations)
- NativeStructures.cs (Structures and enums)
- Unit tests for native interop

**Status After Phase**: Can call Windows APIs

### Phase 2: Process Management (Days 3-4)
**Deliverables**:
- ProcessCreator.cs (Create/resume/terminate)
- Unit tests for process creation
- Manual test with notepad.exe

**Status After Phase**: Can create suspended processes

### Phase 3: DLL Injection (Days 5-6)
**Deliverables**:
- DllInjector.cs (Complete injection logic)
- Test DLL for validation
- Integration tests

**Status After Phase**: Can inject DLLs into processes

### Phase 4: IPC (Days 7-8)
**Deliverables**:
- PipeManager.cs (Named pipe communication)
- Bidirectional message tests
- Integration tests

**Status After Phase**: Can communicate with injected process

### Phase 5: Orchestration (Days 9-10)
**Deliverables**:
- ProcessLaunchOrchestrator.cs (Complete flow)
- Integration with LinBinLauncher
- End-to-end tests

**Status After Phase**: Complete launch flow works

### Phase 6: Finalization (Day 11)
**Deliverables**:
- Documentation complete
- Release notes
- Deployment package

**Status After Phase**: Production ready

---

## Architecture Overview

### Component Diagram

```
┌──────────────────────────────────────────────────────────┐
│                   LinBinLauncher                         │
│            (ILauncherService Implementation)             │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────────────────┐
│            ProcessLaunchOrchestrator                     │
│         (Coordinates entire launch flow)                 │
└───┬────────────────┬────────────────┬────────────────────┘
    │                │                │
    ▼                ▼                ▼
┌─────────┐   ┌──────────────┐   ┌──────────┐
│PipeMgr  │   │ProcessCreator│   │DllInjector│
└─────────┘   └──────────────┘   └─────┬──────┘
                                       │
                                       ▼
                              ┌────────────────┐
                              │ NativeInterop  │
                              │  (Win32 API)   │
                              └────────────────┘
```

### Process Flow

```
1. User clicks "Launch Game"
   ↓
2. LinBinLauncher.LaunchGameAsync()
   ↓
3. ProcessLaunchOrchestrator.LaunchWithInjectionAsync()
   ↓
4. PipeManager.CreatePipesAsync()
   ↓
5. ProcessCreator.CreateSuspended()
   ↓
6. DllInjector.InjectDllsAsync()
   │  ├─ Allocate memory (VirtualAllocEx)
   │  ├─ Write DLL path (WriteProcessMemory)
   │  ├─ Create thread (CreateRemoteThread)
   │  └─ Wait for completion (WaitForSingleObject)
   ↓
7. ProcessCreator.ResumeMainThread()
   ↓
8. PipeManager.WaitForConnectionAsync() [Optional]
   ↓
9. Game Running with DLLs Loaded ✓
```

---

## Component Design

### 1. NativeInterop (Static Class)
**Purpose**: Windows API P/Invoke declarations
**Location**: `src/LineageLauncher.Launcher/Native/NativeInterop.cs`
**Lines of Code**: ~400

**Key Methods**:
- `CreateProcess()` - Create process with flags
- `AllocateMemory()` - VirtualAllocEx wrapper
- `WriteMemory()` - WriteProcessMemory wrapper
- `CreateThread()` - CreateRemoteThread wrapper
- `GetModuleHandle()` - Get DLL handle
- `GetFunctionAddress()` - Get function pointer

### 2. ProcessCreator (Class)
**Purpose**: Manage process lifecycle
**Location**: `src/LineageLauncher.Launcher/Process/ProcessCreator.cs`
**Lines of Code**: ~300

**Key Methods**:
- `CreateSuspended()` - Create process in suspended state
- `ResumeMainThread()` - Resume execution
- `Terminate()` - Kill process
- `Dispose()` - Cleanup resources

### 3. DllInjector (Class)
**Purpose**: Inject DLLs into process
**Location**: `src/LineageLauncher.Launcher/Injection/DllInjector.cs`
**Lines of Code**: ~400

**Key Methods**:
- `InjectDllsAsync()` - Inject multiple DLLs
- `InjectSingleDllAsync()` - Inject one DLL
- `Dispose()` - Cleanup

**Injection Steps**:
1. Allocate memory in target process
2. Write DLL path to allocated memory
3. Get LoadLibraryW address
4. Create remote thread calling LoadLibraryW
5. Wait for thread completion
6. Free allocated memory

### 4. PipeManager (Class)
**Purpose**: Named pipe IPC
**Location**: `src/LineageLauncher.Launcher/IPC/PipeManager.cs`
**Lines of Code**: ~250

**Key Methods**:
- `CreatePipesAsync()` - Create bidirectional pipes
- `WaitForConnectionAsync()` - Wait for client
- `ReadMessageAsync()` - Receive data
- `WriteMessageAsync()` - Send data
- `Dispose()` - Cleanup

### 5. ProcessLaunchOrchestrator (Class)
**Purpose**: Coordinate all components
**Location**: `src/LineageLauncher.Launcher/Orchestration/ProcessLaunchOrchestrator.cs`
**Lines of Code**: ~350

**Key Methods**:
- `LaunchWithInjectionAsync()` - Complete launch flow
- `TerminateAsync()` - Stop game
- `Dispose()` - Cleanup all resources

**Launch Phases**:
1. PipeCreation
2. ProcessCreation
3. DllInjection
4. ThreadResume
5. PipeConnection

---

## Integration Points

### Modify LinBinLauncher.cs

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

**New Code**:
```csharp
// Create launch configuration
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

// Create orchestrator with dependencies
using var orchestrator = new ProcessLaunchOrchestrator(
    _logger,
    new ProcessCreator(_loggerFactory.CreateLogger<ProcessCreator>()),
    new DllInjector(_loggerFactory.CreateLogger<DllInjector>()),
    new PipeManager(_loggerFactory.CreateLogger<PipeManager>()));

// Launch with progress reporting
var progress = new Progress<string>(msg => _logger.LogInformation(msg));
var launchResult = await orchestrator.LaunchWithInjectionAsync(
    config,
    progress,
    cancellationToken);

// Handle result
if (!launchResult.Success)
{
    _logger.LogError(
        "Launch failed at phase {Phase}: {Error}",
        launchResult.FailedPhase,
        launchResult.ErrorMessage);
    return false;
}

_gameProcess = Process.GetProcessById(launchResult.ProcessId);
return true;
```

**Lines Changed**: ~50 (replacement of existing code)

---

## Security Considerations

### Legitimate Use Case

**Important**: This DLL injection is **legitimate** and **required** by the game client architecture. It is NOT for cheating or malicious purposes.

### Anti-Cheat Considerations

1. **Inject Before Anti-Cheat**: Injection occurs in suspended state, before anti-cheat initializes
2. **Official DLLs Only**: Only inject 210916.asi, boxer.dll, libcocos2d.dll (official game DLLs)
3. **Standard API**: Use LoadLibraryW (not manual mapping or other stealth techniques)
4. **Code Signing**: Consider signing launcher executable for authenticity

### DLL Validation

**Before Injection**:
```csharp
private bool ValidateDll(string dllPath)
{
    // Check file exists
    if (!File.Exists(dllPath))
        return false;

    // Verify file size is reasonable
    var fileInfo = new FileInfo(dllPath);
    if (fileInfo.Length == 0 || fileInfo.Length > 100_000_000) // 100MB max
        return false;

    // Optional: Check digital signature
    // Optional: Verify file hash

    return true;
}
```

### Error Recovery

**Critical**: Always terminate suspended process if injection fails

```csharp
try
{
    var injectionResult = await _dllInjector.InjectDllsAsync(...);
    if (!injectionResult.Success)
    {
        _processCreator.Terminate(); // MUST terminate
        return LaunchResult.Failed(...);
    }
}
catch
{
    _processCreator.Terminate(); // MUST terminate
    throw;
}
```

---

## Getting Started

### Prerequisites

1. **Development Environment**:
   - Visual Studio 2022 or VS Code
   - .NET 8.0 SDK
   - Windows 10/11

2. **Knowledge Requirements**:
   - C# language proficiency
   - Understanding of async/await
   - Familiarity with P/Invoke
   - Basic Windows API knowledge

3. **Tools**:
   - Git for version control
   - Process Explorer (optional, for debugging)
   - WinDbg (optional, for advanced debugging)

### Step 1: Read Documentation

**Order**:
1. `DLL_INJECTION_REQUIREMENTS.md` - Understand the problem
2. `DLL_INJECTION_ARCHITECTURE.md` - Learn the solution
3. `DLL_INJECTION_CLASS_TEMPLATES.md` - Review code structure
4. `DLL_INJECTION_TESTING_GUIDE.md` - Understand testing approach
5. `DLL_INJECTION_IMPLEMENTATION_ROADMAP.md` - Follow implementation plan

### Step 2: Set Up Environment

```bash
# Create feature branch
git checkout -b feature/dll-injection

# Create test project
cd tests
dotnet new xunit -n LineageLauncher.Launcher.Tests
cd LineageLauncher.Launcher.Tests
dotnet add reference ../../src/LineageLauncher.Launcher

# Install test packages
dotnet add package FluentAssertions
dotnet add package Moq
```

### Step 3: Start Implementation

Follow **Day 1** in `DLL_INJECTION_IMPLEMENTATION_ROADMAP.md`:
1. Create directory structure
2. Implement NativeInterop.cs
3. Implement NativeStructures.cs
4. Write unit tests
5. Verify compilation

---

## Implementation Order

### Strict Sequential Order

**DO NOT skip ahead**. Each phase builds on the previous:

1. **Day 1**: Native Interop ➔ Foundation for all Win32 calls
2. **Day 2**: Process Creator ➔ Requires native interop
3. **Day 3-4**: DLL Injector ➔ Requires process creator and native interop
4. **Day 5-6**: Pipe Manager ➔ Independent of injection
5. **Day 7-8**: Orchestrator ➔ Requires all components
6. **Day 9**: Integration ➔ Requires orchestrator
7. **Day 10**: Testing ➔ Requires complete implementation
8. **Day 11**: Documentation ➔ Final polish

### Daily Workflow

**Each Day**:
1. Read roadmap for the day
2. Implement code from templates
3. Write unit tests
4. Run tests: `dotnet test`
5. Commit progress: `git commit -m "Day X: [description]"`
6. Update daily standup notes

---

## Code Templates

### Complete templates available in:
`DLL_INJECTION_CLASS_TEMPLATES.md`

**Includes**:
- NativeInterop.cs (~400 lines)
- NativeStructures.cs (~200 lines)
- ProcessCreator.cs (~300 lines)
- DllInjector.cs (~400 lines)
- PipeManager.cs (~250 lines)
- ProcessLaunchOrchestrator.cs (~350 lines)
- Result classes (~100 lines)

**Total**: ~2,000 lines of production-ready code

---

## Testing Strategy

### Test Pyramid

- **Unit Tests**: 75% of tests, fast, isolated
- **Integration Tests**: 20% of tests, component interaction
- **E2E Tests**: 5% of tests, complete flows

### Coverage Target

**90%+ code coverage** for:
- NativeInterop
- ProcessCreator
- DllInjector
- PipeManager
- ProcessLaunchOrchestrator

### Test Phases

1. **Unit Tests**: Test each component in isolation
2. **Integration Tests**: Test components working together
3. **Manual Tests**: Test with real executables (notepad, Lin.bin)
4. **E2E Tests**: Complete launch flows
5. **Performance Tests**: Measure launch time and resource usage

### Key Test Scenarios

**Must Test**:
- ✅ Normal launch (happy path)
- ✅ Missing DLL file
- ✅ Invalid executable path
- ✅ Injection failure (rollback)
- ✅ Cancellation during injection
- ✅ Multiple sequential launches
- ✅ Resource cleanup (no leaks)

---

## Test Plan

### Detailed test plan in:
`DLL_INJECTION_TESTING_GUIDE.md`

**Includes**:
- Unit test examples
- Integration test examples
- Manual test procedures
- E2E test scenarios
- Performance benchmarks
- Troubleshooting guide

---

## Success Metrics

### Technical Metrics

| Metric | Target | Acceptable | Unacceptable |
|--------|--------|------------|--------------|
| Launch Time | < 1s | < 2s | > 3s |
| Test Coverage | > 95% | > 90% | < 80% |
| Memory Usage | < 50MB | < 100MB | > 150MB |
| Handle Leaks | 0 | < 10 | > 50 |
| Crash Rate | 0% | < 0.1% | > 1% |

### Functional Requirements

**Must Have**:
- ✅ Lin.bin launches successfully
- ✅ All 3 DLLs inject correctly
- ✅ Game runs without crashes
- ✅ Error handling works
- ✅ Resource cleanup works

**Should Have**:
- ✅ Named pipes work (optional feature)
- ✅ Progress reporting works
- ✅ Logging is comprehensive

**Nice to Have**:
- ✅ Performance metrics
- ✅ Multiple client versions supported

---

## Risk Assessment

### High-Risk Areas

#### 1. Anti-Cheat Detection
**Likelihood**: Medium
**Impact**: High
**Mitigation**:
- Inject before anti-cheat loads
- Use standard APIs (LoadLibraryW)
- Only inject official DLLs
- Code sign launcher executable

**Contingency**: Fallback to original LWLauncher.exe if detected

#### 2. Handle Leaks
**Likelihood**: Low
**Impact**: Medium
**Mitigation**:
- IDisposable pattern everywhere
- Comprehensive resource cleanup tests
- Handle count monitoring

**Contingency**: Fix leaks in patch release

#### 3. DLL Load Failures
**Likelihood**: Medium
**Impact**: High
**Mitigation**:
- Validate DLLs before injection
- Rollback on failure (terminate process)
- Clear error messages

**Contingency**: Detailed error logging for diagnosis

#### 4. Client Version Changes
**Likelihood**: High
**Impact**: Medium
**Mitigation**:
- Version detection
- DLL path configuration
- Graceful degradation

**Contingency**: Update launcher for new versions

---

## Validation Criteria

### Pre-Release Checklist

**Code Quality**:
- [ ] All files have XML documentation
- [ ] No compiler warnings
- [ ] Code follows C# conventions
- [ ] SOLID principles followed
- [ ] No code duplication

**Testing**:
- [ ] 90%+ unit test coverage
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Manual testing complete
- [ ] Performance benchmarks met
- [ ] No memory leaks detected
- [ ] No handle leaks detected

**Functionality**:
- [ ] Lin.bin launches successfully
- [ ] All DLLs inject correctly
- [ ] Game runs normally
- [ ] Can play for 30+ minutes
- [ ] Multiple launches work
- [ ] Error handling works
- [ ] Rollback works on failure

**Documentation**:
- [ ] Architecture document complete
- [ ] API documentation complete
- [ ] User guide written
- [ ] Troubleshooting guide complete
- [ ] Release notes written
- [ ] Code comments comprehensive

**Deployment**:
- [ ] Builds in Release mode
- [ ] All tests pass in Release
- [ ] Installer tested
- [ ] Clean install tested
- [ ] Rollback plan documented
- [ ] Stakeholders informed

---

## Troubleshooting

### Common Issues

#### "CreateProcess Failed: Access Denied"
**Solution**: Run launcher as administrator

#### "VirtualAllocEx Failed: Error 5"
**Solution**: Check process bitness (32-bit vs 64-bit)

#### "DLL Not Found"
**Solution**: Use absolute paths, verify file exists

#### "Injection Timed Out"
**Solution**: Increase timeout, check DLL dependencies

#### "Pipe Connection Timeout"
**Solution**: This may be normal if game doesn't use pipes

### Debug Logging

Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "LineageLauncher.Launcher": "Debug"
    }
  }
}
```

---

## Resources

### Documentation Files

1. **Requirements**: `DLL_INJECTION_REQUIREMENTS.md`
2. **Architecture**: `DLL_INJECTION_ARCHITECTURE.md`
3. **Templates**: `DLL_INJECTION_CLASS_TEMPLATES.md`
4. **Testing**: `DLL_INJECTION_TESTING_GUIDE.md`
5. **Roadmap**: `DLL_INJECTION_IMPLEMENTATION_ROADMAP.md`

### External Resources

- **Microsoft Docs**: https://docs.microsoft.com/windows/win32/
- **P/Invoke Reference**: https://www.pinvoke.net/
- **Process Hacker**: https://github.com/processhacker/processhacker
- **Named Pipes**: https://docs.microsoft.com/windows/win32/ipc/named-pipes

---

## Support

### Getting Help

1. **Architecture Questions**: Review architecture document
2. **Implementation Help**: Check class templates
3. **Testing Issues**: See testing guide
4. **Bugs**: Check troubleshooting section

### Contact

- **Project Lead**: [Your Name]
- **Repository**: D:\L1R Project\l1r-customlauncher
- **Documentation**: D:\L1R Project\l1r-customlauncher\docs

---

## Conclusion

This complete guide provides everything needed to implement DLL injection for the Lineage custom launcher:

✅ **Comprehensive Documentation**: 5 detailed documents
✅ **Ready-to-Use Code**: ~2,000 lines of templates
✅ **Clear Timeline**: 11-day implementation plan
✅ **Testing Strategy**: Unit, integration, E2E tests
✅ **Risk Mitigation**: Identified and addressed
✅ **Success Criteria**: Clear and measurable

**The architecture is production-ready and ready for implementation.**

---

## Quick Start Command

```bash
# Start implementation today
git checkout -b feature/dll-injection
cd docs
cat DLL_INJECTION_IMPLEMENTATION_ROADMAP.md | grep "Day 1" -A 50

# Begin with Day 1: Foundation Setup
```

---

**Document Status**: Complete and Ready
**Last Updated**: 2025-11-12
**Version**: 1.0
**Next Action**: Begin Day 1 of Implementation Roadmap
