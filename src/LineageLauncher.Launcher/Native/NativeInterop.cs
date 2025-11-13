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
