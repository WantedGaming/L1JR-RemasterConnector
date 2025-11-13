# Complete UI Editing Guide for Lineage Warrior Client

## Current Situation
- UI files are **Cocos2d-x CSB format** (binary compiled UI layouts)
- CSB files have companion `.csb.xml` files (encrypted with XOR)
- We have only 38 bytes of the XOR key, need full key for complete decryption

## Three Approaches to Edit UI

---

## Approach 1: Memory Dump (FASTEST, RECOMMENDED FIRST)

**Pros:** Gets you the full decrypted XML immediately
**Cons:** Requires debugging tools

### Steps:
1. Use **Cheat Engine** or **x64dbg** to attach to LineageWarrior.exe
2. Open a UI element in-game (inventory, character sheet, etc.)
3. Search memory for string: `<?xml version="1.0" encoding="UTF-8"?>`
4. Dump the full memory region containing the XML
5. Compare with encrypted file to extract full XOR key

**See:** `memory_intercept_guide.md` for detailed instructions

**Time estimate:** 1-2 hours with tools setup

---

## Approach 2: Reverse Engineer Client Executable

**Pros:** Gets you the encryption key for future use
**Cons:** Requires reverse engineering skills

### Steps:
1. Run the key search tool:
   ```cmd
   cd "D:\L1R Project\tools"
   python find_xor_key_in_exe.py "D:\L1R Project\LineageWarriorClient\LineageWarrior"
   ```

2. The tool will:
   - Search for the 38-byte key segment we already know
   - Extract surrounding bytes (likely contains full key)
   - Try common key lengths (64, 128, 256 bytes)
   - Save candidate keys to test

3. Test each candidate key:
   ```cmd
   python test_xor_key.py <candidate_key.bin> <encrypted.csb.xml>
   ```

4. Verify decrypted XML is valid

**Time estimate:** 2-4 hours

---

## Approach 3: Edit CSB Directly (NO DECRYPTION NEEDED)

**Pros:** Skip encryption entirely, use official Cocos tools
**Cons:** CSB is binary format, need Cocos Studio

### Option A: Use Cocos Studio (Official Tool)

**Cocos Studio** is the official editor for CSB files:

1. **Download Cocos Studio**:
   - Legacy version: https://www.cocos.com/en/cocos-studio (discontinued but still works)
   - Alternative: Cocos Creator 3.x (newer, but may not support old CSB)

2. **Open CSB files**:
   - Extract CSB from .pak archives first
   - Open in Cocos Studio: File → Open → Select .csb
   - Edit visually (move elements, change text, resize, etc.)
   - Export back to CSB

3. **Repack into .pak**:
   - Use pak unpacker/repacker tools
   - Replace edited CSB in pak archive

**ISSUE:** Lineage may use a custom/modified CSB format that Cocos Studio can't read.

### Option B: CSB Binary Parser (Python)

Create a parser to read/modify CSB directly:

```python
# CSB files are FlatBuffers format (Google's serialization)
# Structure (typical):
# - Header (magic bytes, version)
# - FlatBuffer binary data
# - Resources table
# - Node tree (UI hierarchy)

import flatbuffers

# Parse CSB
with open('inventory.csb', 'rb') as f:
    data = f.read()

# Check magic bytes
if data[:4] == b'CSB\x00':  # or similar
    print("Valid CSB file")

# Parse FlatBuffer structure
# (requires .fbs schema definition)
```

**ISSUE:** We don't have the FlatBuffers schema (.fbs) for Lineage's CSB format.

### Option C: Hex Edit CSB (Text Strings Only)

If you only need to change visible text:

1. Open CSB in **HxD Hex Editor**
2. Search for text strings (UTF-8 encoded)
3. Replace in-place (keep same byte length)
4. Save and test

**Limitations:**
- Can only change text content
- Cannot move/resize UI elements
- Must keep exact same string length

---

## Approach 4: Skip XML, Parse CSB Structure

If CSB files contain the same data as XML (just compiled):

### Create CSB Parser:

```python
#!/usr/bin/env python3
"""
Parse Lineage CSB files to extract UI structure.
CSB = Cocos Studio Binary format
"""

import struct
from pathlib import Path

class CSBParser:
    def __init__(self, csb_path):
        self.path = Path(csb_path)
        with open(csb_path, 'rb') as f:
            self.data = f.read()

        self.offset = 0
        self.header = self.parse_header()

    def parse_header(self):
        """Parse CSB file header"""
        # Common CSB header structure:
        # 4 bytes: magic ("CSB\x00" or similar)
        # 4 bytes: version
        # 4 bytes: total size
        # ... (varies by Cocos version)

        magic = self.data[0:4]
        version = struct.unpack('<I', self.data[4:8])[0]
        size = struct.unpack('<I', self.data[8:12])[0]

        return {
            'magic': magic,
            'version': version,
            'size': size
        }

    def find_text_strings(self):
        """Find all text strings in CSB"""
        strings = []
        i = 0
        while i < len(self.data) - 4:
            # Look for length-prefixed strings (common in FlatBuffers)
            str_len = struct.unpack('<I', self.data[i:i+4])[0]

            if 0 < str_len < 1000:  # Reasonable string length
                try:
                    text = self.data[i+4:i+4+str_len].decode('utf-8')
                    if text.isprintable():
                        strings.append({
                            'offset': i,
                            'length': str_len,
                            'text': text
                        })
                        i += 4 + str_len
                        continue
                except:
                    pass

            i += 1

        return strings

# Usage:
parser = CSBParser('inventory.csb')
print(f"CSB Version: {parser.header['version']}")
print(f"Text strings found:")
for s in parser.find_text_strings():
    print(f"  Offset 0x{s['offset']:X}: {s['text']}")
```

This lets you:
- Extract all text from CSB
- Locate string positions for hex editing
- Understand file structure without decryption

---

## RECOMMENDED WORKFLOW

### Phase 1: Quick Text Changes (TODAY)
1. Extract CSB files from .pak
2. Use hex editor to find/replace text strings
3. Keep same byte length (pad with spaces if needed)
4. Repack and test

### Phase 2: Get Full Decryption Key (THIS WEEK)
1. Try memory dump approach first (fastest)
2. If that fails, search executables for full key
3. Create batch decryption tool once key is found

### Phase 3: Full UI Editing (LONG TERM)
1. With full key, decrypt all XML files
2. Edit XML in any text editor
3. Re-encrypt and replace in .pak
4. OR: Reverse engineer CSB format completely

---

## Tools Needed

### For Memory Dump:
- Cheat Engine: https://www.cheatengine.org/
- x64dbg: https://x64dbg.com/

### For Hex Editing:
- HxD: https://mh-nexus.de/en/hxd/
- 010 Editor: https://www.sweetscape.com/010editor/ (paid, has CSB template)

### For CSB Editing:
- Cocos Studio (legacy): Find archived download
- Cocos Creator: https://www.cocos.com/en/creator/download

### For PAK Archives:
- Check if existing unpacker supports repacking
- May need to create custom repacker

---

## Current Assets

You already have:
- ✅ 38-byte XOR key (partial)
- ✅ pak_reader.py (can extract files)
- ✅ Python environment
- ✅ Decrypted .tbl files (for text translation)

You need:
- ❌ Full XOR key (64-256 bytes)
- ❌ CSB editor or parser
- ❌ pak repacker tool

---

## Next Steps

**Choose your path:**

1. **Need quick text fixes?** → Use hex editor on CSB files
2. **Want proper XML editing?** → Get full XOR key via memory dump
3. **Want to understand file format?** → Reverse engineer CSB structure

**My recommendation as an NCsoft veteran:**
Start with memory dump. It's the fastest way to get full decrypted XMLs, and once you have those, you can edit freely and re-encrypt with the full key. The XML is the source of truth; CSB is just compiled version.
