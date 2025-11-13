# Memory Interception Guide for Lineage XML Decryption

## Goal
Intercept decrypted XML files in memory as the client loads them.

## Tools Required
- **Cheat Engine** (free, powerful memory scanner)
- **x64dbg** or **OllyDbg** (debuggers)
- **Process Monitor** (Sysinternals) to track file access

## Step-by-Step Process

### Phase 1: Identify XML Loading Function

1. **Launch LineageWarrior client** with debugger attached
2. **Set breakpoint on file read APIs**:
   ```
   CreateFileW
   ReadFile
   fopen
   ```

3. **Trigger UI loading** (open inventory, character window, etc.)
4. **Check call stack** when .csb.xml files are accessed
5. **Find the decryption function** (look for XOR loops in disassembly)

### Phase 2: Memory Dump of Decrypted Data

**Method A: Break on Memory Write**
```
1. Set hardware breakpoint on XML file buffer (after ReadFile)
2. Step through until XOR decryption loop completes
3. Dump memory buffer to file
4. Buffer should contain full decrypted XML
```

**Method B: Search for XML Strings**
```
1. Open Cheat Engine, attach to LineageWarrior.exe
2. Memory Scan -> Array of byte -> UTF-8 string
3. Search for: "<?xml version"
4. Filter results for those followed by readable XML tags
5. Right-click valid result -> Browse this memory region
6. Memory Viewer -> Hex view -> Tools -> Dissect data/structures
7. Copy entire XML structure to file
```

**Method C: Hook File Read Function**
```c
// DLL injection approach - hook fread/ReadFile
BOOL WINAPI ReadFileHook(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead,
                         LPDWORD lpNumberOfBytesRead, LPOVERLAPPED lpOverlapped) {
    BOOL result = ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, lpNumberOfBytesRead, lpOverlapped);

    // Check if this is an XML file read
    if (IsXMLFile(hFile)) {
        // Buffer now contains encrypted data
        DecryptBuffer(lpBuffer, *lpNumberOfBytesRead);
        // lpBuffer now has decrypted XML - dump to disk
        DumpToFile("decrypted_xml.xml", lpBuffer, *lpNumberOfBytesRead);
    }
    return result;
}
```

### Phase 3: Extract Full XOR Key

Once you have:
- Original encrypted .csb.xml file
- Decrypted XML from memory

Extract the full key:
```python
def extract_full_key(encrypted_path, decrypted_path):
    with open(encrypted_path, 'rb') as f:
        encrypted = f.read()
    with open(decrypted_path, 'rb') as f:
        decrypted = f.read()

    # XOR to get full key
    key_length = min(len(encrypted), len(decrypted))
    full_key = bytes(e ^ d for e, d in zip(encrypted[:key_length], decrypted[:key_length]))

    # Check if key repeats
    for test_len in [64, 128, 256, 512]:
        if is_repeating_key(full_key, test_len):
            print(f"Key repeats every {test_len} bytes")
            return full_key[:test_len]

    return full_key

def is_repeating_key(data, period):
    """Check if key repeats with given period"""
    if len(data) < period * 2:
        return False
    for i in range(len(data) - period):
        if data[i] != data[i + period]:
            return False
    return True
```

## Expected Outcomes

**If key is 64 bytes:**
- You'll see pattern repeat every 64 bytes in extracted key
- Common for older Lineage clients

**If key is 256 bytes:**
- More secure, used in newer clients
- May not see full pattern if file < 512 bytes

**If key includes file header:**
- First N bytes might be file-specific (checksum, length)
- True XOR key starts after header

## Tool Setup

### Cheat Engine Download
https://www.cheatengine.org/

### x64dbg Download
https://x64dbg.com/

### Frida (Advanced - Dynamic Instrumentation)
```bash
pip install frida-tools
frida-trace -p <pid> -i "ReadFile" -i "CreateFileW"
```

## Next Steps After Success

1. Verify key works on ALL .csb.xml files
2. Create batch decryption tool
3. Edit XML files (UI layouts, text, positioning)
4. Re-encrypt with same key
5. Test in client
