# Lin.bin Launch Flow Diagrams

**Visual representation of Lineage client launch process**

---

## 1. Basic Launch Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    User Starts Launcher                         │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Pre-Launch Validation                              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Check Lin270.exe exists                                  │  │
│  │ Check PAK files (data, sprite, image, ui)               │  │
│  │ Check VC++ Runtime (MSVCP140.dll)                       │  │
│  │ Check DirectX Runtime (d3d9.dll)                        │  │
│  └──────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                ┌───────────┴───────────┐
                │ Validation Failed?    │
                └───────┬───────────┬───┘
                        │           │
                    ✗ YES       ✓ NO
                        │           │
                        ▼           ▼
              ┌─────────────┐   ┌─────────────────────────────┐
              │ Show Error  │   │ Prepare Launch Environment  │
              │ Install     │   │ - Set working directory     │
              │ Missing     │   │ - Build command line        │
              │ Components  │   │ - Optional: Copy DLLs       │
              └─────────────┘   └──────────┬──────────────────┘
                                           │
                                           ▼
                              ┌────────────────────────────────┐
                              │ CreateProcess(Lin270.exe)      │
                              │ Args: "192.168.1.100 2000"     │
                              │ WorkingDir: "C:\Lineage"       │
                              └──────────┬─────────────────────┘
                                         │
                                         ▼
                              ┌────────────────────────────────┐
                              │ Lin270.exe Process Starts      │
                              └──────────┬─────────────────────┘
                                         │
                                         ▼
                              ┌────────────────────────────────┐
                              │ Launcher Monitors Process      │
                              │ - Check for crashes            │
                              │ - Log errors                   │
                              │ - Wait for exit                │
                              └──────────┬─────────────────────┘
                                         │
                                         ▼
                              ┌────────────────────────────────┐
                              │ Game Exits                     │
                              │ Cleanup & Close Launcher       │
                              └────────────────────────────────┘
```

---

## 2. Lin.bin Internal Initialization

```
┌─────────────────────────────────────────────────────────────────┐
│                 Lin270.exe WinMain() Entry                      │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Parse Command-Line Arguments                       │
│  argv[1] = "192.168.1.100"  (Server IP)                        │
│  argv[2] = "2000"            (Server Port)                     │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Windows Subsystem Initialization                   │
│  - Register window class                                        │
│  - Create main game window (800x600 or fullscreen)             │
│  - Set window title "Lineage"                                  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Load Configuration                                 │
│  - Read config.ini (if exists)                                 │
│  - Load graphics settings (resolution, fullscreen, etc.)       │
│  - Load audio settings (volume, effects)                       │
│  - Load game settings (username cache, etc.)                   │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              DirectX Initialization                             │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ Call Direct3DCreate9()                                  │  │
│  │ ├─> Windows loads d3d9.dll                             │  │
│  │ │   (Custom d3d9.dll in folder? → Load that first)     │  │
│  │ │   (No custom? → Load System32\d3d9.dll)              │  │
│  │ └─> Create D3D9 device                                 │  │
│  │     Create render targets, depth buffer                 │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              DirectInput Initialization                         │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ Call DirectInput8Create()                               │  │
│  │ ├─> Windows loads dinput8.dll                          │  │
│  │ │   (Custom dinput8.dll in folder? → Load that)        │  │
│  │ └─> Create keyboard and mouse devices                  │  │
│  │     Set cooperative level                               │  │
│  │     Acquire devices                                     │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Load Game Data (PAK Files)                         │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ Open data.pak and data.idx                              │  │
│  │ Open sprite00-15.pak/idx                                │  │
│  │ Open image00-15.pak/idx                                 │  │
│  │ Open ui.pak/idx, icon.pak/idx, tile.pak/idx            │  │
│  │                                                          │  │
│  │ For each PAK:                                           │  │
│  │ 1. Decrypt index with 30-byte XOR key                  │  │
│  │ 2. Parse ARMS format index structure                   │  │
│  │ 3. Load frequently-used files into memory               │  │
│  │ 4. Decrypt file data with file-level XOR (if needed)   │  │
│  │ 5. Decompress ZLIB data                                 │  │
│  │                                                          │  │
│  │ Parse desc-server.tbl (localization)                    │  │
│  │ Parse desc-item.tbl, desc-skill.tbl                     │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Audio Initialization                               │
│  - Load DirectSound (dsound.dll)                               │
│  - Initialize audio buffers                                    │
│  - Load background music files (*.mp3, *.ogg)                  │
│  - Load sound effects (*.wav)                                  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Network Initialization                             │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ Create TCP socket                                       │  │
│  │ Connect to argv[1]:argv[2] (192.168.1.100:2000)        │  │
│  │ Send client version packet                              │  │
│  │ Wait for server handshake response                      │  │
│  │                                                          │  │
│  │ If connection successful:                               │  │
│  │   → Receive encryption keys from server                 │  │
│  │   → Initialize packet handlers                          │  │
│  │                                                          │  │
│  │ If connection fails:                                    │  │
│  │   → Show "Connection timeout" error                     │  │
│  │   → Return to login screen                              │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Display Login Screen                               │
│  - Render UI from ui.pak                                       │
│  - Show username/password fields                               │
│  - Show server name (from command-line or config)              │
│  - Play background music                                       │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Main Game Loop                                     │
│  while (running) {                                             │
│    ProcessInput()      // Keyboard, mouse via DirectInput      │
│    UpdateGame()        // Game logic, network packets          │
│    RenderFrame()       // DirectX rendering                    │
│    ProcessMessages()   // Windows messages                     │
│  }                                                             │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Korean Launcher (CtoolNt) Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                 User Runs Generated Launcher.exe                │
│                 (Created by CtoolNt)                            │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Read Embedded Configuration                        │
│  server_name = "My Private Server"                            │
│  server_ip   = "192.168.1.100"                                │
│  server_port = 2000                                            │
│  auto_login  = false (optional)                                │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Pre-Launch Operations                              │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ Check if Lineage is already running                     │  │
│  │ └─> If yes: Show error, exit                           │  │
│  │                                                          │  │
│  │ Verify Lin270.exe exists                                │  │
│  │ └─> If no: Show error, exit                            │  │
│  │                                                          │  │
│  │ Optional: Modify Windows hosts file                     │  │
│  │ └─> Add: 127.0.0.1  lineage.plaync.com                 │  │
│  │     (For GameGuard bypass)                              │  │
│  │                                                          │  │
│  │ Optional: Copy wrapper DLLs                             │  │
│  │ └─> Copy d3d9.dll to game folder                       │  │
│  │     Copy dinput8.dll to game folder                     │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Build Command Line                                 │
│  commandLine = server_ip + " " + server_port                   │
│  Example: "192.168.1.100 2000"                                │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Launch Lin270.exe via CreateProcess                │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ STARTUPINFO si;                                         │  │
│  │ PROCESS_INFORMATION pi;                                 │  │
│  │ ZeroMemory(&si, sizeof(si));                            │  │
│  │ ZeroMemory(&pi, sizeof(pi));                            │  │
│  │                                                          │  │
│  │ CreateProcess(                                          │  │
│  │   "C:\\Lineage\\Lin270.exe",  // lpApplicationName      │  │
│  │   "192.168.1.100 2000",       // lpCommandLine          │  │
│  │   NULL,                        // lpProcessAttributes    │  │
│  │   NULL,                        // lpThreadAttributes     │  │
│  │   FALSE,                       // bInheritHandles        │  │
│  │   0,                           // dwCreationFlags        │  │
│  │   NULL,                        // lpEnvironment          │  │
│  │   "C:\\Lineage",              // lpCurrentDirectory     │  │
│  │   &si,                         // lpStartupInfo          │  │
│  │   &pi                          // lpProcessInformation   │  │
│  │ );                                                       │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Optional: DLL Injection                            │
│  (Only if launcher includes custom features)                   │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ Wait for Lin270.exe to initialize (Sleep(2000))         │  │
│  │                                                          │  │
│  │ Allocate memory in Lin270 process:                      │  │
│  │   pDllPath = VirtualAllocEx(...)                        │  │
│  │                                                          │  │
│  │ Write DLL path to allocated memory:                     │  │
│  │   WriteProcessMemory(pDllPath, "custom.dll")            │  │
│  │                                                          │  │
│  │ Get LoadLibraryA address:                               │  │
│  │   pLoadLib = GetProcAddress("kernel32", "LoadLibraryA") │  │
│  │                                                          │  │
│  │ Create remote thread to load DLL:                       │  │
│  │   CreateRemoteThread(pi.hProcess, pLoadLib, pDllPath)   │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Monitor Game Process                               │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │ while (true) {                                          │  │
│  │   DWORD exitCode;                                       │  │
│  │   GetExitCodeProcess(pi.hProcess, &exitCode);           │  │
│  │                                                          │  │
│  │   if (exitCode != STILL_ACTIVE) {                       │  │
│  │     // Game exited                                      │  │
│  │     break;                                              │  │
│  │   }                                                     │  │
│  │                                                          │  │
│  │   Sleep(1000);  // Check every second                   │  │
│  │ }                                                        │  │
│  └─────────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              Cleanup and Exit                                   │
│  - Remove wrapper DLLs (if copied)                             │
│  - Restore hosts file (if modified)                            │
│  - Close process handles                                       │
│  - Exit launcher                                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. Dependency Resolution Flow

```
                    ┌──────────────────────┐
                    │  Check VC++ Runtime  │
                    └──────────┬───────────┘
                               │
                ┌──────────────┴──────────────┐
                │                             │
            ✓ Found                       ✗ Missing
                │                             │
                │                             ▼
                │              ┌──────────────────────────────┐
                │              │ Show Dialog:                 │
                │              │ "Visual C++ Runtime Missing" │
                │              │ [Download & Install] [Cancel]│
                │              └──────────┬───────────────────┘
                │                         │
                │                         ▼
                │              ┌──────────────────────────────┐
                │              │ Download VC++ Redistributable│
                │              │ from Microsoft               │
                │              │ (vc_redist.x86.exe)         │
                │              └──────────┬───────────────────┘
                │                         │
                │                         ▼
                │              ┌──────────────────────────────┐
                │              │ Execute installer:           │
                │              │ ShellExecute(vc_redist.exe)  │
                │              │ Wait for completion          │
                │              └──────────┬───────────────────┘
                │                         │
                └─────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │  Check DirectX       │
                    └──────────┬───────────┘
                               │
                ┌──────────────┴──────────────┐
                │                             │
            ✓ Found                       ✗ Missing
                │                             │
                │                             ▼
                │              ┌──────────────────────────────┐
                │              │ Show Dialog:                 │
                │              │ "DirectX Runtime Missing"    │
                │              │ [Download & Install] [Cancel]│
                │              └──────────┬───────────────────┘
                │                         │
                │                         ▼
                │              ┌──────────────────────────────┐
                │              │ Download DirectX Runtime     │
                │              │ from Microsoft               │
                │              │ (directx_Jun2010_redist.exe) │
                │              └──────────┬───────────────────┘
                │                         │
                │                         ▼
                │              ┌──────────────────────────────┐
                │              │ Execute installer:           │
                │              │ ShellExecute(directx.exe)    │
                │              │ Wait for completion          │
                │              └──────────┬───────────────────┘
                │                         │
                └─────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │  All Dependencies OK │
                    │  Ready to Launch     │
                    └──────────────────────┘
```

---

## 5. Error Handling Flow

```
                    ┌──────────────────────┐
                    │  Launch Attempted    │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────────────────┐
                    │  CreateProcess() Return Value    │
                    └──────────┬───────┬───────────────┘
                               │       │
                          ✓ Success  ✗ Failed
                               │       │
                               │       ▼
                               │  ┌────────────────────────────┐
                               │  │ Get Error Code             │
                               │  │ dwError = GetLastError()   │
                               │  └──────────┬─────────────────┘
                               │             │
                               │             ▼
                               │  ┌─────────────────────────────────┐
                               │  │ Switch (dwError) {              │
                               │  │                                 │
                               │  │ ERROR_FILE_NOT_FOUND:           │
                               │  │   "Lin270.exe not found"        │
                               │  │                                 │
                               │  │ ERROR_ACCESS_DENIED:            │
                               │  │   "Run as Administrator"        │
                               │  │                                 │
                               │  │ ERROR_BAD_EXE_FORMAT:           │
                               │  │   "Corrupted executable"        │
                               │  │                                 │
                               │  │ default:                        │
                               │  │   "Unknown error: {dwError}"    │
                               │  │ }                                │
                               │  └──────────┬──────────────────────┘
                               │             │
                               │             ▼
                               │  ┌─────────────────────────────────┐
                               │  │ Show Error Dialog               │
                               │  │ Log to error.log                │
                               │  │ Return to launcher main screen  │
                               │  └─────────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────────────────┐
                    │ Game Process Started             │
                    └──────────┬───────────────────────┘
                               │
                               ▼
                    ┌──────────────────────────────────┐
                    │ Monitor Process                  │
                    │ WaitForSingleObject(hProcess)    │
                    └──────────┬───────────────────────┘
                               │
                               ▼
                    ┌────────────────────────────────────┐
                    │ Process Exited                     │
                    │ GetExitCodeProcess()               │
                    └──────────┬───────┬─────────────────┘
                               │       │
                          exitCode==0  exitCode!=0
                          (Normal)    (Crash)
                               │       │
                               │       ▼
                               │  ┌────────────────────────────┐
                               │  │ Crash Detected             │
                               │  │ - Check error.log          │
                               │  │ - Show crash dialog        │
                               │  │ - Offer to send report     │
                               │  └────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │  Cleanup & Exit      │
                    └──────────────────────┘
```

---

## 6. PAK File Loading Sequence

```
Lin.bin Loads PAK Files:

  ┌─────────────────┐
  │  Open data.idx  │
  └────────┬────────┘
           │
           ▼
  ┌──────────────────────────────┐
  │ Decrypt index with XOR key   │
  │ (30-byte PAK-level key)      │
  └────────┬─────────────────────┘
           │
           ▼
  ┌──────────────────────────────┐
  │ Parse ARMS format structure  │
  │ - File count                 │
  │ - File offsets               │
  │ - File sizes                 │
  │ - Compression flags          │
  └────────┬─────────────────────┘
           │
           ▼
  ┌──────────────────────────────┐
  │ Open data.pak                │
  └────────┬─────────────────────┘
           │
           ▼
  ┌──────────────────────────────┐
  │ For each file in index:      │
  │                              │
  │ 1. Seek to offset            │
  │ 2. Read encrypted bytes      │
  │ 3. XOR decrypt (if XML/text) │
  │ 4. Decompress (ZLIB)         │
  │ 5. Store in memory           │
  └────────┬─────────────────────┘
           │
           ▼
  ┌──────────────────────────────┐
  │ Repeat for:                  │
  │ - sprite00-15.pak            │
  │ - image00-15.pak             │
  │ - ui.pak                     │
  │ - icon.pak                   │
  │ - tile.pak                   │
  └────────┬─────────────────────┘
           │
           ▼
  ┌──────────────────────────────┐
  │ All PAKs loaded successfully │
  │ Ready for rendering          │
  └──────────────────────────────┘
```

---

## Legend

```
┌────────┐
│ Action │  = Process or operation
└────────┘

    │
    ▼       = Flow direction

    ┌───┐
    │ ? │   = Decision point
    └─┬─┘

  ✓ / ✗     = Success / Failure

[Button]    = User interaction

// Comment  = Code or explanation
```

---

**Related Documents:**
- [Complete Research](./LIN_BIN_LAUNCH_MECHANICS.md)
- [Quick Reference](./LIN_BIN_QUICK_REFERENCE.md)

**Last Updated:** 2025-11-11
