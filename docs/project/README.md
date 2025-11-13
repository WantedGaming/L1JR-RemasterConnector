# L1R Project - Clean Structure

This folder contains all the components for the L1R (Lineage Remastered) private server project.

## Folder Structure

```
L1R Project/
├── .claude/                    # Claude Code configuration
├── L1J-WantedServer/          # L1J game server (Java)
├── LineageWarriorClient/      # Game client with launcher
├── L1R-CustomLauncher/        # Modern .NET launcher (In Development) ⭐ NEW
├── L1R-SpawnManager/          # Standalone spawn/drop management tool
├── L1MapEditor/               # Map editor utility
├── L1R-PAK-Tools/             # PAK file extraction tools
├── L1R-PAK-Editor/            # PAK file editing suite
├── L1RDatabase/               # Laravel web interface
├── CLAUDE.md                  # Project instructions for Claude
└── PROJECT_STRUCTURE.md       # Original project documentation
```

## Main Components

### 🎮 L1J-WantedServer
The main game server based on L1J (Lineage Java). This handles:
- Player connections and authentication
- Game logic and mechanics
- Database operations
- NPC AI and spawning
- Drop systems
- Web app center

**Location:** `L1J-WantedServer/L1J-Wanted/`

### 🖥️ LineageWarriorClient
The game client and launcher for players to connect to the server.

**Location:** `LineageWarriorClient/`

### 🚀 L1R-CustomLauncher (NEW - In Development)
**Modern .NET 8 launcher to replace LWLauncher.exe**

**Features:**
- Fast startup (~1-2 seconds vs 3-5 seconds)
- Modern security (Argon2id password hashing)
- Efficient patching (parallel downloads, Zstandard compression)
- Windows 11 Fluent Design UI
- Embedded browser for news (WebView2)
- Clean Architecture + MVVM pattern

**Location:** `L1R-CustomLauncher/`

**Documentation:**
- See `L1R-CustomLauncher/README.md` for details
- See `L1R-CustomLauncher/CHANGELOG.md` for version history
- See `L1R-CustomLauncher/docs/` for comprehensive guides

**Status:** Phase 1 - Documentation & Planning

### ⚙️ L1R-SpawnManager
**Standalone Spring Boot application for managing spawns and drops.**

**Features:**
- Dark theme admin UI
- REST API for programmatic access
- Spawn management (create, edit, delete, search)
- Drop management with advanced filtering
- Connects to L1J database
- Runs independently on port 8080

**Location:** `L1R-SpawnManager/`

**Quick Start:**
```cmd
cd L1R-SpawnManager
build.bat
run.bat
```

**Access:** http://localhost:8080

### 🗺️ L1MapEditor
Map editor utility for creating and modifying game maps.

**Location:** `L1MapEditor/`

## Getting Started

### For Server Administrators

1. **Start L1J Server:**
   ```cmd
   cd "L1J-WantedServer\L1J-Wanted"
   startserver.bat
   ```

2. **Start Spawn Manager (Optional):**
   ```cmd
   cd "L1R-SpawnManager"
   run.bat
   ```

3. **Configure Client:**
   - Point client to your server IP
   - Launch with `LWLauncher.exe`

### For Developers

See individual README files in each component folder for detailed instructions.

## Database

All components connect to the same MySQL database: `l1j_remastered`

**Tables:**
- `spawnlist` - Monster/mob spawns
- `spawnlist_npc` - NPC spawns
- `droplist` - Item drop configuration
- `characters` - Player data
- And many more...

## Documentation

- `CLAUDE.md` - Project instructions and architecture for Claude Code
- `PROJECT_STRUCTURE.md` - Detailed project structure documentation
- Each component has its own README with specific instructions

## Notes

- All components are designed to work together but can run independently
- The Spawn Manager is completely standalone and doesn't modify server code
- Server and client are in separate folders for clean organization
- Database is shared between server and spawn manager

---

**Last Updated:** November 9, 2025
