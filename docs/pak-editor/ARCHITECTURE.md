# L1R PAK Editor - Technical Architecture

**Version**: 1.0.0-alpha
**Last Updated**: 2025-11-08

---

## System Overview

```
┌──────────────────────────────────────────────────────────┐
│                    L1R PAK Editor                         │
│                  (WinForms Application)                   │
├──────────────────────────────────────────────────────────┤
│                                                           │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────┐  │
│  │   Main UI   │  │  XML Editor  │  │   CSB Editor   │  │
│  │   Window    │  │              │  │                │  │
│  └──────┬──────┘  └──────┬───────┘  └───────┬────────┘  │
│         │                │                   │           │
│         └────────────────┴───────────────────┘           │
│                          │                               │
│         ┌────────────────┴────────────────┐              │
│         │                                 │              │
│  ┌──────▼──────┐                   ┌──────▼──────┐      │
│  │ PAK Manager │                   │ File Manager│      │
│  │  (Core)     │                   │   (Core)    │      │
│  └──────┬──────┘                   └──────┬──────┘      │
│         │                                 │              │
│  ┌──────▼──────────────────────────────────▼──────┐     │
│  │          Encryption / Compression Module       │     │
│  │  - XOR (PAK-level: 39 bytes)                   │     │
│  │  - XOR (File-level: 55 bytes)                  │     │
│  │  - ZLIB compression/decompression              │     │
│  └──────┬─────────────────────────────────────────┘     │
│         │                                                │
│  ┌──────▼──────────────────────────────────────────┐    │
│  │            File System / I/O Layer              │    │
│  │  - IDX Reader/Writer                            │    │
│  │  - PAK Reader/Writer                            │    │
│  │  - Memory Dump Reader (optional)                │    │
│  └─────────────────────────────────────────────────┘    │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

---

## Core Components

### 1. PAK Manager (Core/PakManager.cs)

**Responsibilities**:
- Read IDX index files
- Extract files from PAK archives
- Write PAK/IDX archives
- Manage file metadata

**Key Classes**:
```csharp
public class PakManager
{
    public List<PakEntry> Entries { get; }
    public void ReadIndex(string idxPath, string pakPath);
    public byte[] ExtractFile(string filename);
    public void ExtractAll(string outputDir);
    public void PackFiles(string sourceDir, string idxPath, string pakPath);
}

public class PakEntry
{
    public string Filename { get; set; }
    public int CumulativeOffset { get; set; }
    public int UncompressedSize { get; set; }
    public int CompressedSize { get; set; }
    public int CompressionFlag { get; set; }  // 0 = None, 2 = ZLIB
    public int ActualOffset { get; set; }
}
```

### 2. Encryption Module (Core/Encryption.cs)

**Responsibilities**:
- XOR encryption/decryption (PAK-level)
- XOR encryption/decryption (File-level)
- Key management

**Keys**:
```csharp
// PAK-level XOR key (39 bytes) - Used BEFORE ZLIB compression
private static readonly byte[] PAK_XOR_KEY = {
    0x64, 0x00, 0x00, 0x00, 0xDD, 0x04, 0x1C, 0xA1,
    0xD8, 0x99, 0xD6, 0xBA, 0x14, 0x79, 0x94, 0x00,
    0x1A, 0x16, 0xC1, 0x96, 0x31, 0xD1, 0x25, 0x49,
    0x63, 0xB5, 0x4D, 0xD6, 0xA5, 0xF8, 0xE2, 0x90,
    0xE9, 0x4B, 0xD2, 0x9F, 0x99, 0xEB, 0x91
};

// File-level XOR key (55 bytes) - Used AFTER extraction from PAK
private static readonly byte[] FILE_XOR_KEY = {
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0xBC, 0x99, 0xD6, 0xBA, 0xC9, 0x7D, 0x88, 0xA1,
    0x7E, 0x16, 0xC1, 0x96, 0xEC, 0xD5, 0x39, 0xE8,
    0x07, 0xB5, 0x4D, 0xD6, 0x78, 0xFC, 0xFE, 0x31,
    0x8D, 0x4B, 0xD2, 0x9F, 0x44, 0xEF, 0x8D, 0xBC,
    0xEE, 0x03, 0x7D, 0x93, 0xEE, 0x1F, 0x12, 0x27,
    0x87, 0x1A, 0x42, 0x5F, 0x6E, 0x9C, 0x07
};
```

### 3. Compression Module (Core/Compression.cs)

**Responsibilities**:
- ZLIB compression
- ZLIB decompression

**Implementation**:
```csharp
public static class CompressionHelper
{
    public static byte[] Compress(byte[] data);
    public static byte[] Decompress(byte[] compressed, int expectedSize);
}
```

### 4. File Manager (Core/FileManager.cs)

**Responsibilities**:
- Track opened files
- Manage file modifications
- Handle save operations
- Temporary file management

---

## UI Components

### 1. Main Window (UI/MainForm.cs)

**Layout**:
```
┌────────────────────────────────────────────────────┐
│  File  Edit  Tools  Help                           │
├────────────────────────────────────────────────────┤
│  [Open PAK] [Extract] [Repack] [Settings]          │
├────────────┬───────────────────────────────────────┤
│            │                                        │
│  File Tree │         Content Preview/Editor        │
│            │                                        │
│  📁 ui.pak │  ┌──────────────────────────────────┐ │
│  ├─ 📄 2k_ │  │                                  │ │
│  │  ChatUI │  │     [File Content Here]          │ │
│  ├─ 📄 Acti│  │                                  │ │
│  │  onScene│  │                                  │ │
│  └─ ...    │  └──────────────────────────────────┘ │
│            │                                        │
├────────────┴───────────────────────────────────────┤
│  Status: Ready  |  File: None  |  Size: 0 bytes    │
└────────────────────────────────────────────────────┘
```

**Features**:
- Dark theme (System.Drawing with custom colors)
- Split panel layout (file tree + editor)
- Toolbar with common actions
- Status bar with file info

### 2. XML Editor (UI/XmlEditor.cs)

**Features**:
- Syntax highlighting (tags, attributes, values)
- Line numbers
- Find/Replace
- Auto-formatting
- Validation

**Implementation**:
- Use `FastColoredTextBox` NuGet package for syntax highlighting
- Or custom `RichTextBox` with manual coloring

### 3. CSB Editor (UI/CsbEditor.cs)

**Features** (Phase 2):
- Hex view mode
- Visual editor mode (if CSB format decoded)
- Property inspector
- Preview rendering

---

## Data Flow

### Extraction Flow

```
1. User opens PAK/IDX file
   ↓
2. PakManager.ReadIndex()
   - Parse IDX file
   - Build entry list
   ↓
3. User selects file to extract
   ↓
4. PakManager.ExtractFile()
   - Read from PAK at offset
   - XOR decrypt (PAK-level)
   - ZLIB decompress (if flag=2)
   ↓
5. Check if file-level encryption
   ↓
6. If XML file:
   - XOR decrypt (File-level, 55-byte key)
   - Load into XML editor
   ↓
7. Display in editor
```

### Repacking Flow

```
1. User modifies file in editor
   ↓
2. User clicks "Save" or "Repack"
   ↓
3. FileManager.PrepareForPacking()
   - If XML: XOR encrypt (File-level)
   - If compressed: ZLIB compress
   - XOR encrypt (PAK-level)
   ↓
4. PakManager.PackFiles()
   - Calculate offsets
   - Write IDX entries
   - Write PAK data
   ↓
5. Validate new archive
   ↓
6. Success notification
```

---

## File Format Specifications

### IDX File Structure

```
┌────────────────────────────────────────┐
│  Header (16 bytes)                     │
│  - Magic: "ARMS" (4 bytes)             │
│  - Version: int32 (4 bytes)            │
│  - Reserved: int32 (4 bytes)           │
│  - File Count: int32 (4 bytes)         │
├────────────────────────────────────────┤
│  8-byte prefix (unknown purpose)       │
├────────────────────────────────────────┤
│  Entry 0 (276 bytes)                   │
│  - Filename (260 bytes, null-term)     │
│  - Cumulative Offset (4 bytes)         │
│  - Uncompressed Size (4 bytes)         │
│  - Compressed Size (4 bytes)           │
│  - Compression Flag (4 bytes)          │
├────────────────────────────────────────┤
│  Entry 1 (276 bytes)                   │
│  ...                                   │
└────────────────────────────────────────┘
```

### PAK File Structure

```
┌────────────────────────────────────────┐
│  File 0 Data (encrypted/compressed)    │
├────────────────────────────────────────┤
│  File 1 Data (encrypted/compressed)    │
├────────────────────────────────────────┤
│  ...                                   │
└────────────────────────────────────────┘
```

**Offset Calculation**:
```csharp
actualOffset = cumulativeOffset - fileCount
```

---

## Technology Choices

### Why .NET 9.0?
- Latest C# 12 features
- Best performance (AOT compilation)
- Improved WinForms designer
- Native Windows integration

### Why WinForms over WPF?
- Simpler, faster for this use case
- Better performance for file operations
- Easier dark theme implementation
- Native look and feel

### Why Not Electron/Web?
- Unnecessary overhead
- Slower file I/O
- Larger memory footprint
- Windows-only tool doesn't need cross-platform

---

## Security Considerations

### Encryption Keys

**Storage**:
- Hardcoded in source (acceptable for private tool)
- Not user-configurable
- Documented in code comments

**Usage**:
- PAK-level key for archive encryption
- File-level key for XML/CSB encryption
- Keys never transmitted or exposed

### File Operations

**Safety**:
- Backup original files before repacking
- Validate modified data before writing
- Error handling for corrupted archives
- Temporary files cleaned up on exit

---

## Performance Targets

- **Open PAK**: < 1 second for 300-file archive
- **Extract Single File**: < 100ms for typical XML
- **Extract All Files**: < 10 seconds for 300 files
- **Repack Archive**: < 5 seconds for 300 files
- **Memory Usage**: < 500MB for typical workload

---

## Testing Strategy

### Unit Tests
- PakManager read/write operations
- Encryption/decryption correctness
- Compression/decompression correctness
- Offset calculations

### Integration Tests
- Full extract → edit → repack cycle
- Validation against original archives
- Game client compatibility testing

### Test Data
- `TestData/decrypted_*.xml` - Known good files
- Real `ui.pak` from game client
- Corrupted archives for error handling

---

## Future Enhancements

### Phase 3+ Features
- Plugin system for custom editors
- Scripting API for automation
- Batch translation interface
- Diff/Merge tool for comparing archives
- Memory dump integration (live game editing)
- Hot-reload support (instant game updates)

---

**Status**: 🚧 Architecture Design Complete
**Next**: Implementation of Core/PakManager.cs
