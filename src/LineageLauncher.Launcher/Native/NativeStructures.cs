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
