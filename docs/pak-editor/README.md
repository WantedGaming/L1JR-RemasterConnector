# L1R PAK Editor - All-in-One Tool

**Version**: 1.0.0-alpha.2
**Platform**: Windows
**Framework**: .NET 9.0
**UI**: WinForms with Dark Theme
**Status**: ✅ Milestone 1 Complete - Core PAK Engine Working!

## Overview

Complete all-in-one tool for editing Lineage Remastered PAK/IDX archives and UI files.

**Replaces**: PackViewer.exe + manual editing workflow
**Provides**: Extract → Edit → Repack in single application

## Current Capabilities (v1.0.0-alpha.2)

✅ **Working Now:**
- Read and parse ARMS format IDX index files
- Extract files from PAK archives with full decryption
- PAK-level XOR decryption (39-byte key)
- File-level XOR decryption (55-byte key) for XML files
- ZLIB decompression for compressed files
- Command-line interface for all operations
- Successfully tested with ui.pak (3,578 files)
- Encryption/decryption verified with round-trip testing

🔨 **Coming Next:**
- WinForms GUI with dark theme
- XML editor with syntax highlighting
- Repack functionality to create modified PAK files

## Features

### Phase 1: PAK Archive Management ✅ (Milestone 1 Complete!)
- [x] Read IDX index files
- [x] Extract files from PAK archives
- [x] XOR decryption (PAK-level: 39 bytes)
- [x] XOR decryption (File-level: 55 bytes)
- [x] ZLIB decompression
- [x] Command-line test interface
- [ ] Repack modified files into PAK
- [ ] Create new PAK/IDX archives

### Phase 2: File Editing (In Progress)
- [ ] XML Editor with syntax highlighting
- [ ] CSB (Cocos Studio) visual editor
- [ ] Hex editor for binary files
- [ ] Image viewer/editor for sprites
- [ ] Text search and replace

### Phase 3: Advanced Features (Planned)
- [ ] Korean → English translation system
- [ ] Batch file operations
- [ ] Diff/Compare tool
- [ ] Memory extraction integration
- [ ] One-click mod deployment

## Technology Stack

- **Language**: C# 12 (.NET 9.0)
- **UI Framework**: Windows Forms
- **Theme**: Custom dark theme
- **XML Parsing**: System.Xml.Linq
- **Compression**: System.IO.Compression

## Quick Start

### Build the Project
```cmd
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"
dotnet build
```

### Command-Line Test Interface

The project includes a command-line test mode for PAK operations:

**List all files in a PAK archive:**
```cmd
dotnet run -- --test list "path\to\ui.idx" "path\to\ui.pak"
```

**Extract a single file:**
```cmd
dotnet run -- --test extract "path\to\ui.idx" "path\to\ui.pak" "2k_ChatUI.xml"
```

**Extract all files:**
```cmd
dotnet run -- --test extractall "path\to\ui.idx" "path\to\ui.pak" "output_directory"
```

**Encrypt a file (file-level encryption):**
```cmd
dotnet run -- --test encrypt-file "input.xml" "output_encrypted.xml"
```

**Decrypt a file (file-level encryption):**
```cmd
dotnet run -- --test decrypt-file "encrypted.xml" "output_decrypted.xml"
```

### Example: Extract ui.pak
```cmd
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"
dotnet run -- --test extractall "D:\L1R Project\LineageWarriorClient\ui.idx" "D:\L1R Project\LineageWarriorClient\ui.pak" "extracted_ui"
```

This will extract all 3,578 files from ui.pak to the `extracted_ui` directory.

## Project Structure

```
L1R-PAK-Editor/
├── L1RPakEditor/
│   ├── Core/              ← PAK/IDX handling, encryption, compression
│   ├── UI/                ← WinForms UI components
│   ├── Docs/              ← Research documentation
│   ├── Resources/         ← Icons, themes, assets
│   └── Program.cs         ← Entry point
│
├── TestData/              ← Sample decrypted XML files
│   ├── decrypted_1209ccb4.xml
│   ├── decrypted_43f7d0ac.xml
│   └── decrypted_43fa10cc.xml
│
├── README.md              ← This file
├── CHANGELOG.md           ← Version history
└── ARCHITECTURE.md        ← Technical design

```

## Development Plan

### Milestone 1: Core PAK Engine ✅ COMPLETE
- ✅ Project setup
- ✅ PakManager class (read IDX/PAK)
- ✅ Encryption/Decryption utilities (PAK-level + File-level)
- ✅ Compression utilities (ZLIB)
- ✅ Command-line test interface
- ✅ Tested with real ui.pak (3578 files)
- [ ] PakWriter class (write IDX/PAK) - deferred to later
- [ ] Unit tests - deferred to later

### Milestone 2: Basic UI (Week 2)
- [ ] Main window with dark theme
- [ ] File tree view (PAK contents)
- [ ] Extract file functionality
- [ ] Preview pane
- [ ] Status bar and logging

### Milestone 3: XML Editor (Week 3)
- [ ] Syntax-highlighted XML editor
- [ ] File-level XOR decryption integration
- [ ] Memory extraction integration
- [ ] Save and encrypt functionality

### Milestone 4: Repacking & Testing (Week 4)
- [ ] Repack modified files into PAK
- [ ] Validate repacked archives
- [ ] Test in game client
- [ ] Bug fixes and polish

## Related Research

See `Docs/` folder for complete technical documentation:
- **01-PAK-FORMAT-RESEARCH.md** - Complete PAK/IDX format specification
- **MEMORY-EXTRACTION-BREAKTHROUGH-README.md** - Memory extraction technique

## License

Private development tool for L1R server project.

---

**Status**: 🚧 Active Development
**Last Updated**: 2025-11-08
