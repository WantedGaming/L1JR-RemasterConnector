# L1R UI Editing Tools

This folder contains editing tools for Lineage Remastered UI files.

## Tools Overview

### 1. **L1RXmlEditor** - Visual XML Editor
A modern dark-themed WinForms application for editing the 270+ XML UI files.

**Features:**
- ✅ Tree view of XML structure
- ✅ Property grid for editing attributes
- ✅ Direct XML text editing
- ✅ Syntax highlighting (selected nodes highlight in text editor)
- ✅ Search functionality (Ctrl+F)
- ✅ Dark theme (VS Code style)
- ✅ Expand/Collapse all nodes
- ✅ Auto-formatted XML

**Usage:**
```cmd
cd L1RXmlEditor
dotnet run
```

Then use File → Open to select an XML file from:
`D:\L1R Project\L1R-PAK-Tools\extracted from client\xml\`

**Workflow:**
1. Open an XML file (e.g., `2k_ChatUI.decoded.xml`)
2. Browse the tree structure in the left panel
3. Select an element to view/edit properties (auto-highlights in text editor)
4. Edit XML directly in the text box on the right
5. Save changes

---

### 2. **CsbConverter** - Cocos Studio Binary Converter
A command-line tool for analyzing and converting CSB files (Cocos Studio Binary format).

**Features:**
- ✅ Display CSB file information
- ✅ Extract version and structure details
- ✅ Convert CSB to JSON (simplified representation)
- ⏳ Convert JSON to CSB (requires FlatBuffers schema)

**Usage:**
```cmd
cd CsbConverter

# Show CSB file info
dotnet run info "path/to/file.csb"

# Convert CSB to JSON
dotnet run to-json "path/to/file.csb"

# Convert CSB to JSON with custom output
dotnet run to-json "path/to/file.csb" "output.json"
```

**Example:**
```cmd
dotnet run info "D:\L1R Project\L1R-PAK-Tools\extracted from client\csb\ActionScene.csb"
```

**Output:**
```
Cocos Studio Version: 2.1.0.0
Size: 764 bytes
Contains: UI Node hierarchy
```

---

## Current Status

### XML Editing: ✅ FULLY WORKING
- All 270 XML files are already decrypted and ready to edit
- Use **L1RXmlEditor** for visual editing
- Or use any text editor (VS Code, Notepad++)

### CSB Editing: ⚠️ PARTIAL
- CSB files can be analyzed and converted to JSON
- **Full CSB editing requires:**
  1. Cocos Studio 2.x IDE (official tool)
  2. FlatBuffers schema (.fbs files) from the game client
  3. Custom CSB editor (complex to build)

**Recommended Workflow for CSB:**
1. Use **CsbConverter** to analyze structure
2. Extract strings and basic data
3. For full visual editing: Install **Cocos Studio 2.1** (if available)

---

## File Locations

**Decrypted XML files:**
```
D:\L1R Project\L1R-PAK-Tools\extracted from client\xml\*.decoded.xml
```

**CSB files:**
```
D:\L1R Project\L1R-PAK-Tools\extracted from client\csb\**\*.csb
```

---

## Next Steps for CSB Full Support

To enable full CSB editing (convert JSON → CSB), we need:

1. **Extract FlatBuffers Schema** from game client
   - Search in `Lin.bin` for `.fbs` files or schema definitions
   - Or reverse engineer from CSB file structure

2. **Use Cocos Studio IDE**
   - Download Cocos Studio 2.1 (if available from Cocos2d-x archives)
   - Open .csb files directly for visual editing
   - Export back to .csb format

3. **Build Custom CSB Editor** (advanced)
   - Create WinForms/WPF visual UI editor
   - Implement FlatBuffers serialization
   - Support drag-and-drop UI element positioning

---

## Quick Start Guide

### Editing XML UI Files

1. **Launch XML Editor:**
   ```cmd
   cd "D:\L1R Project\L1R-PAK-Editor\L1RXmlEditor"
   dotnet run
   ```

2. **Open a file:**
   - Click **File → Open**
   - Navigate to `D:\L1R Project\L1R-PAK-Tools\extracted from client\xml\`
   - Select any `.decoded.xml` file

3. **Edit:**
   - Browse tree structure (left panel)
   - Select nodes to highlight them in the text editor (dark yellow)
   - View/edit properties (middle panel)
   - Edit raw XML (right panel)

4. **Save:**
   - Click **File → Save** (Ctrl+S)

### Analyzing CSB Files

```cmd
cd "D:\L1R Project\L1R-PAK-Editor\CsbConverter"

# Show info about a CSB file
dotnet run info "D:\L1R Project\L1R-PAK-Tools\extracted from client\csb\ActionScene.csb"

# Convert to JSON for inspection
dotnet run to-json "D:\L1R Project\L1R-PAK-Tools\extracted from client\csb\ActionScene.csb"

# View the generated JSON
notepad ActionScene.json
```

---

## Known Limitations

1. **CSB to JSON**: Currently creates a simplified representation, not a full UI description
2. **JSON to CSB**: Not yet implemented (requires FlatBuffers schema)
3. **Visual CSB Editor**: Not built (use Cocos Studio IDE if needed)

---

## Tools Built

| Tool | Status | Purpose |
|------|--------|---------|
| **L1RXmlEditor** | ✅ Complete | Visual XML editor with dark theme |
| **CsbConverter** | ⚠️ Partial | CSB info and basic JSON conversion |
| **PackViewer.exe** | ✅ Available | PAK extraction (existing tool) |

---

## Contributing

If you find the FlatBuffers schema files (.fbs) for Cocos Studio 2.1.0.0, please add them to enable full CSB editing!

**Last Updated:** 2025-11-08
