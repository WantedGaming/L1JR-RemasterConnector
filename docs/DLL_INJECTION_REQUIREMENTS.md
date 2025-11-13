# DLL Injection Requirements for Lineage Lin.bin Launch

## Date: 2025-11-12
## Source: Analysis of working LWLauncher.exe

## Executive Summary

Lin.bin (Lineage Remaster client) **requires DLL injection** to launch properly. Simple `Process.Start()` does not work because the client expects specific DLLs to be injected into its process space before execution.

## Working Launcher Analysis

### Process Flow from LWLauncher.log:
```
1. Create GameStart
2. GameStart.Start Creating protocol...
   - Create_Protocol Creating objects...
   - Create_Protocol End
3. GameStart.Start Creating injection...
4. GameStart.Start Starting pipe1...
   - Pipe1Start1 Starting pipe1...
   - Pipe1Start1 Pipe1 started.
5. GameStart.Start Starting pipe2...
   - Pipe1Start Starting pipe2...
6. Pipe1Start2 creating process...
   - Create_Process Start
   - Create_Process Trying to create process...
   - Create_Process End
7. Pipe1Start2 process created
8. Pipe1Start2 injection done
9. GameStart.Start End.
```

### Key Components Required:

1. **Protocol/Objects Setup**
   - Communication protocol initialization
   - Object creation for injection mechanism

2. **Named Pipes (IPC)**
   - **Pipe 1**: Launcher → Game communication
   - **Pipe 2**: Game → Launcher communication
   - Used for bidirectional communication during and after launch

3. **Process Creation (Suspended)**
   - Create Lin.bin process in SUSPENDED state
   - This allows DLL injection before the main thread executes

4. **DLL Injection**
   - Inject required DLLs into the suspended process
   - DLLs include: 210916.asi, boxer.dll, libcocos2d.dll

5. **Process Resume**
   - Resume main thread after injection complete
   - Game starts with injected DLLs loaded

## Technical Implementation Requirements

### Windows API Functions Needed (P/Invoke):

#### Process Creation:
```csharp
[DllImport("kernel32.dll")]
static extern bool CreateProcess(
    string lpApplicationName,
    string lpCommandLine,
    IntPtr lpProcessAttributes,
    IntPtr lpThreadAttributes,
    bool bInheritHandles,
    ProcessCreationFlags dwCreationFlags,
    IntPtr lpEnvironment,
    string lpCurrentDirectory,
    ref STARTUPINFO lpStartupInfo,
    out PROCESS_INFORMATION lpProcessInformation
);

[Flags]
enum ProcessCreationFlags : uint
{
    CREATE_SUSPENDED = 0x00000004,
    CREATE_NEW_CONSOLE = 0x00000010,
    // ... other flags
}
```

#### DLL Injection:
```csharp
[DllImport("kernel32.dll")]
static extern IntPtr OpenProcess(
    ProcessAccessFlags dwDesiredAccess,
    bool bInheritHandle,
    int dwProcessId
);

[DllImport("kernel32.dll")]
static extern IntPtr VirtualAllocEx(
    IntPtr hProcess,
    IntPtr lpAddress,
    uint dwSize,
    AllocationType flAllocationType,
    MemoryProtection flProtect
);

[DllImport("kernel32.dll")]
static extern bool WriteProcessMemory(
    IntPtr hProcess,
    IntPtr lpBaseAddress,
    byte[] lpBuffer,
    uint nSize,
    out int lpNumberOfBytesWritten
);

[DllImport("kernel32.dll")]
static extern IntPtr GetProcAddress(
    IntPtr hModule,
    string lpProcName
);

[DllImport("kernel32.dll")]
static extern IntPtr GetModuleHandle(
    string lpModuleName
);

[DllImport("kernel32.dll")]
static extern IntPtr CreateRemoteThread(
    IntPtr hProcess,
    IntPtr lpThreadAttributes,
    uint dwStackSize,
    IntPtr lpStartAddress,
    IntPtr lpParameter,
    uint dwCreationFlags,
    out IntPtr lpThreadId
);
```

#### Thread Management:
```csharp
[DllImport("kernel32.dll")]
static extern uint ResumeThread(IntPtr hThread);

[DllImport("kernel32.dll")]
static extern bool CloseHandle(IntPtr hObject);
```

### Named Pipe Implementation:

```csharp
using System.IO.Pipes;

// Pipe 1: Launcher → Game
var pipe1Server = new NamedPipeServerStream(
    "LineageLauncher_Pipe1",
    PipeDirection.Out,
    1,
    PipeTransmissionMode.Byte
);

// Pipe 2: Game → Launcher
var pipe2Server = new NamedPipeServerStream(
    "LineageLauncher_Pipe2",
    PipeDirection.In,
    1,
    PipeTransmissionMode.Byte
);
```

## DLL Injection Process

### Step-by-Step Flow:

1. **Setup Phase:**
   ```
   - Create named pipe servers (pipe1, pipe2)
   - Start listening on pipes
   - Prepare DLL paths for injection
   ```

2. **Process Creation Phase:**
   ```
   - Build command line and environment
   - Create process with CREATE_SUSPENDED flag
   - Store process handle and main thread handle
   ```

3. **Injection Phase:**
   ```
   For each DLL (210916.asi, boxer.dll, libcocos2d.dll):
     a. Allocate memory in remote process (VirtualAllocEx)
     b. Write DLL path to remote memory (WriteProcessMemory)
     c. Get LoadLibraryA address from kernel32.dll
     d. Create remote thread to call LoadLibraryA (CreateRemoteThread)
     e. Wait for remote thread to complete
     f. Clean up remote memory
   ```

4. **Resume Phase:**
   ```
   - Resume main thread (ResumeThread)
   - Close handles
   - Monitor pipe communication
   ```

## Security Considerations

### Anti-Cheat Detection:
- DLL injection is a common cheat technique
- Our injection is **legitimate** (required by client)
- Must inject BEFORE anti-cheat initializes
- Use official DLLs only (210916.asi, boxer.dll from server)

### Error Handling:
- Handle injection failures gracefully
- Terminate suspended process if injection fails
- Log all injection steps for debugging
- Provide clear error messages to user

## Files to Create/Modify

### New Files:
1. `src/LineageLauncher.Launcher/DllInjector.cs`
   - Core injection logic
   - Windows API P/Invoke declarations

2. `src/LineageLauncher.Launcher/ProcessCreator.cs`
   - Create process with CREATE_SUSPENDED
   - Handle process lifetime

3. `src/LineageLauncher.Launcher/PipeManager.cs`
   - Named pipe server/client
   - Bidirectional communication

4. `src/LineageLauncher.Launcher/NativeStructures.cs`
   - STARTUPINFO, PROCESS_INFORMATION
   - Enums and flags

### Modified Files:
1. `src/LineageLauncher.Launcher/LinBinLauncher.cs`
   - Replace simple Process.Start() with injection flow
   - Integrate DllInjector, ProcessCreator, PipeManager

## Testing Strategy

### Phase 1: Process Creation
- Test CREATE_SUSPENDED process creation
- Verify process starts suspended
- Verify process can be resumed

### Phase 2: Memory Allocation
- Test VirtualAllocEx in remote process
- Verify memory is writable
- Test WriteProcessMemory

### Phase 3: Single DLL Injection
- Inject boxer.dll only
- Verify DLL loads successfully
- Check for errors

### Phase 4: Multi-DLL Injection
- Inject all required DLLs
- Test injection order
- Verify all DLLs load

### Phase 5: Named Pipes
- Test pipe creation
- Test pipe communication
- Handle connection/disconnection

### Phase 6: Full Integration
- Complete launch flow
- Test with real Lin.bin
- Monitor for crashes/hangs

## Reference Documentation

### Microsoft Docs:
- CreateProcess: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw
- VirtualAllocEx: https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualallocex
- WriteProcessMemory: https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-writeprocessmemory
- CreateRemoteThread: https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createremotethread
- Named Pipes: https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipes

### Related Projects:
- Process Hacker: https://github.com/processhacker/processhacker
- Deviare In-Proc: https://github.com/nektra/Deviare-InProc
- EasyHook: https://github.com/EasyHook/EasyHook

## Implementation Timeline

1. **Day 1-2**: Architecture design and P/Invoke setup
2. **Day 3-4**: Process creation and suspension
3. **Day 5-6**: DLL injection mechanism
4. **Day 7-8**: Named pipe communication
5. **Day 9-10**: Integration and testing

## Success Criteria

- [ ] Lin.bin launches successfully with injected DLLs
- [ ] No crashes or hangs during injection
- [ ] Named pipes communicate bidirectionally
- [ ] Process resumes correctly after injection
- [ ] Error handling works for all failure modes
- [ ] Logging captures all injection steps
- [ ] Works consistently across multiple launches

## Notes

- This is a **legitimate** injection for game client functionality
- We are NOT creating cheats or hacks
- DLL injection is **required** by the client architecture
- All injected DLLs are official/server-provided
