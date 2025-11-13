# Lin.bin Quick Reference Guide

**Quick lookup for Lin.bin launch implementation**

---

## TL;DR - Minimum Launch Implementation

```csharp
// C# minimum viable launch code
var psi = new ProcessStartInfo
{
    FileName = @"C:\Lineage\Lin270.exe",
    Arguments = "192.168.1.100 2000",  // IP PORT
    WorkingDirectory = @"C:\Lineage",
    UseShellExecute = false
};
Process.Start(psi);
```

---

## Required Files Checklist

### Must Have (Won't launch without these)
```
✓ Lin270.exe (or Lin.bin, EPU.bin)
✓ data.pak + data.idx
✓ sprite00.pak + sprite00.idx
✓ image00.pak + image00.idx
✓ ui.pak + ui.idx
✓ Visual C++ 2015-2019 Runtime (x86)
✓ DirectX End-User Runtime (June 2010)
```

### Should Have (Functional but limited)
```
○ icon.pak + icon.idx
○ tile.pak + tile.idx
○ sprite01-15.pak/idx
○ image01-15.pak/idx
○ music/ folder
○ sound/ folder
```

---

## DLL Dependencies Quick Fix

### Visual C++ Runtime Missing Error
```
Error: "MSVCP140.dll is missing"
Fix: Install VC++ 2015-2019 Redistributable (x86)
Link: https://aka.ms/vs/17/release/vc_redist.x86.exe
```

### DirectX Missing Error
```
Error: "d3dx9_*.dll is missing" or DirectX initialization failed
Fix: Install DirectX End-User Runtime (June 2010)
Link: https://www.microsoft.com/en-us/download/details.aspx?id=8109
```

---

## Command-Line Syntax

### Basic Launch
```batch
Lin270.exe [IP] [PORT]
```

### Examples
```batch
# Local server
Lin270.exe 127.0.0.1 2000

# LAN server
Lin270.exe 192.168.1.100 2000

# Public server
Lin270.exe myserver.com 2000

# Official server (no parameters)
Lin270.exe
```

---

## Executable Variants

| File | Client Version | Usage |
|------|----------------|-------|
| `Lin.bin` | 2.7 Classic | Legacy servers |
| `Lin270.exe` | 2.7 & 2.8 | Most private servers |
| `EPU.bin` | 2.8+ / Remaster | Advanced servers |
| `Lineage.exe` | Official | NCSOFT servers |

**Recommendation:** Use `Lin270.exe` for maximum compatibility

---

## Typical Folder Structure

```
C:\Lineage\
├── Lin270.exe          ← Main executable
├── config.ini          ← Optional config
├── data\
│   ├── data.pak
│   └── data.idx
├── sprite\
│   ├── sprite00.pak
│   └── sprite00.idx
├── image\
│   ├── image00.pak
│   └── image00.idx
├── ui\
│   ├── ui.pak
│   └── ui.idx
├── icon\
│   ├── icon.pak
│   └── icon.idx
└── tile\
    ├── tile.pak
    └── tile.idx
```

---

## C# Launch Implementation Template

```csharp
public class LineageLauncher
{
    public Process LaunchGame(string gameFolder, string ip, int port)
    {
        // Validate
        string exePath = Path.Combine(gameFolder, "Lin270.exe");
        if (!File.Exists(exePath))
            throw new FileNotFoundException("Lin270.exe not found");

        // Check dependencies
        if (!IsVCRuntimeInstalled())
            throw new Exception("Visual C++ Runtime not installed");

        if (!IsDirectXInstalled())
            throw new Exception("DirectX Runtime not installed");

        // Launch
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = $"{ip} {port}",
            WorkingDirectory = gameFolder,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        return Process.Start(psi);
    }

    private bool IsVCRuntimeInstalled()
    {
        // Check for MSVCP140.dll in System32
        string dll = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
            "MSVCP140.dll"
        );
        return File.Exists(dll);
    }

    private bool IsDirectXInstalled()
    {
        // Check for d3d9.dll in System32
        string dll = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
            "d3d9.dll"
        );
        return File.Exists(dll);
    }
}
```

---

## Common Launch Errors

| Error Message | Cause | Solution |
|---------------|-------|----------|
| `MSVCP140.dll missing` | VC++ Runtime | Install VC++ 2015-2019 (x86) |
| `d3dx9_*.dll missing` | DirectX | Install DirectX June 2010 |
| `Can't read lin.bin` | Permission issue | Run as Administrator |
| `Connection timeout` | Server offline | Check server status |
| `Already running` | Process not closed | Kill in Task Manager |
| Black screen | Graphics driver | Update drivers / Windowed mode |

---

## PAK File Encryption (For Reference)

**Do NOT need to decrypt PAK files for launching**
Lin.bin handles decryption automatically.

If you need to modify PAK files:
- Use L1R-PAK-Tools (Java or C# editor)
- 30-byte XOR key discovered (see PAK-TOOLS documentation)
- File-level: 55-byte XOR for XML files

---

## Korean Launcher Terms

| Korean (한글) | English | Notes |
|--------------|---------|-------|
| 리니지 | Lineage | The game |
| 실행기/접속기 | Launcher | Launcher app |
| 프리서버 | Private Server | Unofficial server |
| 씨툴 (CtoolNt) | CTool | Launcher creator tool |

---

## Advanced: DLL Injection (Optional)

**⚠️ Only for advanced modifications - Not required for basic launcher**

### Proxy DLL Method
```
Place custom d3d9.dll in game folder
Windows loads it before System32 version
Your DLL loads real d3d9.dll and forwards calls
```

### CreateRemoteThread Method
```cpp
// 1. Launch Lin270.exe
// 2. Get process handle
// 3. Allocate memory for DLL path
// 4. Write DLL path
// 5. CreateRemoteThread with LoadLibraryA
```

**Recommendation:** Avoid DLL injection unless absolutely necessary

---

## Testing Checklist

```
Before Release:
☐ Test on Windows 10
☐ Test on Windows 11
☐ Test without VC++ Runtime (should fail gracefully)
☐ Test without DirectX (should fail gracefully)
☐ Test with local server (127.0.0.1)
☐ Test with LAN server
☐ Test with invalid IP (should timeout gracefully)
☐ Test multiple launches (shouldn't conflict)
☐ Test admin vs non-admin
☐ Test with antivirus enabled
```

---

## Quick Links

**Full Documentation:**
- [Complete Lin.bin Launch Mechanics](./LIN_BIN_LAUNCH_MECHANICS.md)

**Project Documentation:**
- [Launcher Architecture](./ARCHITECTURE.md)
- [Encryption Details](./ENCRYPTION.md)
- [Server Integration](./SERVER-INTEGRATION.md)

**Runtime Downloads:**
- [VC++ 2015-2019 (x86)](https://aka.ms/vs/17/release/vc_redist.x86.exe)
- [DirectX June 2010](https://www.microsoft.com/en-us/download/details.aspx?id=8109)

---

**Last Updated:** 2025-11-11
**Related:** LIN_BIN_LAUNCH_MECHANICS.md (full research document)
