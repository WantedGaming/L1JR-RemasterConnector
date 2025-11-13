# L1R PAK Tools

Tools and utilities for working with Lineage Remastered ARMS PAK/IDX archive files.

## Project Structure

```
L1R-PAK-Tools/
├── docs/           - Documentation and research findings
├── src/            - Main source code (PAK reader/extractor)
├── tools/          - Utility tools (key validator, memory scanner)
└── research/       - Analysis and experimental code
```

## Overview

This project provides tools to work with Lineage Remastered's ARMS (Archive Resource Management System) format, which consists of:
- **`.idx`** files - Index metadata (unencrypted, 276-byte fixed entries)
- **`.pak`** files - Data archives (XOR encrypted, optionally ZLIB compressed)

## Current Status

- **IDX Format**: 100% understood and documented
- **PAK Decryption**: Partial (39-byte XOR key discovered)
- **File Extraction**: Use PackViewer.exe for 100% extraction
- **Editing Tools**: In development (CSB, SPX, TIL editors planned)

## Quick Start

### Extracting Files

**Using PackViewer.exe** (Recommended):
```
PackViewer.exe
```
- Full extraction capability
- No editing support

**Using Our Tools** (Partial - 31% success rate):
```bash
cd src
javac *.java
java PakExtractorTest
```

### Validating XOR Keys

```bash
cd tools
javac KeyValidator.java
java KeyValidator <key_file.bin>
```

### Scanning Memory Dumps

```bash
cd tools
javac DumpFileKeyExtractor.java
java DumpFileKeyExtractor <dump_file.dmp>
```

## Documentation

- **[01-PAK-FORMAT-RESEARCH.md](docs/01-PAK-FORMAT-RESEARCH.md)** - Complete PAK format documentation
- **[MEMORY_DUMP_INSTRUCTIONS.md](docs/MEMORY_DUMP_INSTRUCTIONS.md)** - How to create process memory dumps
- **[PLAN_B_EXTRACTION.md](docs/PLAN_B_EXTRACTION.md)** - Alternative extraction methods
- **[WHICH_PROCESS_TO_DUMP.md](docs/WHICH_PROCESS_TO_DUMP.md)** - Process identification guide

## Main Components

### Source Code (`src/`)

**ArmsPakReader.java**
- Reads ARMS IDX/PAK archives
- Implements XOR decryption
- Handles ZLIB decompression
- Works with partial 39-byte key (31% extraction rate)

**PakEntry.java**
- Data model for PAK file entries
- Stores filename, offset, size, compression flag

**PakExtractorTest.java**
- Test program for PAK extraction
- Outputs extraction statistics

### Tools (`tools/`)

**KeyValidator.java**
- Validates XOR encryption keys
- Tests key quality against known XML files
- Supports hex string or binary file input

**DumpFileKeyExtractor.java**
- Scans memory dumps for XOR key patterns
- Tests multiple key lengths (64-1024 bytes)
- Validates extracted keys automatically

### Research (`research/`)

Contains experimental code and analysis tools used during reverse engineering:
- Key derivation experiments
- Compression testing
- Statistical analysis
- Brute force attempts

## File Format Specifications

### IDX File Structure
```
Header (16 bytes):
  - Magic: "ARMS" (4 bytes)
  - Version: int32 (4 bytes)
  - Reserved: int32 (4 bytes)
  - File Count: int32 (4 bytes)

8-byte prefix (unknown purpose)

Entries (276 bytes each):
  - Filename: null-terminated string (260 bytes)
  - Metadata (last 16 bytes):
    - Cumulative Offset: uint32
    - Uncompressed Size: int32
    - Compressed Size: int32
    - Compression Flag: int32 (0=uncompressed, 2=ZLIB)
```

### PAK File Structure
```
Data files stored sequentially:
  - XOR encrypted
  - Optionally ZLIB compressed
  - Offset calculation: actualOffset = cumulativeOffset - fileCount
```

### XOR Key (Partial - 39 bytes)
```
64 00 00 00 DD 04 1C A1 D8 99 D6 BA 14 79 94 00
1A 16 C1 96 31 D1 25 49 63 B5 4D D6 A5 F8 E2 90
E9 4B D2 9F 99 EB 91
```

## File Types in PAK Archives

- **XML** - UI layouts and configurations (uncompressed)
- **CSB** - Cocos Studio Binary (compiled UI, ZLIB compressed)
- **SPX** - Sprite files (custom format)
- **TIL** - Tile map data

## Roadmap

- [ ] Complete XOR key discovery (128-256 bytes)
- [ ] Build CSB (Cocos Studio Binary) editor
- [ ] Build SPX (Sprite) viewer/editor
- [ ] Build TIL (Tile) editor
- [ ] Create PAK repacker tool
- [ ] Design comprehensive UI Editor with JavaFX

## Development Approach

**Short-term**: Use PackViewer.exe for extraction, work with extracted files

**Long-term**: Create custom PAK format for private server (no NCsoft dependency)

## Requirements

- Java 8+ (Java 11+ recommended)
- Windows OS
- PackViewer.exe (for full extraction)

## Contributing

This is a private server development project. Tools are designed for:
- Modding Lineage Remastered client files
- Creating custom content for private servers
- Translating Korean content to English

## License

Research and development tools for private server hosting. Not affiliated with NCsoft.

---

**Last Updated**: November 2025
**Status**: Active development - Editor suite in planning phase
