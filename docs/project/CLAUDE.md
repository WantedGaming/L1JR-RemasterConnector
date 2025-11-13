# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Environment**: This project is developed and run on Windows OS. All commands and paths are Windows-specific.

## Agent Usage Guidelines

When working with this codebase, utilize specialized agents for optimal results:

**Java Development:**
- Use `lineage-server-developer` agent for L1J-specific modifications (game mechanics, NPC systems, packets, server features)
- Use `java-expert-developer` agent for complex Java architecture, async patterns, performance optimization, or deep technical challenges
- Use `ncsoft-lineage-expert` agent for game design decisions requiring Lineage franchise knowledge

**Code Quality:**
- Use `code-reviewer` agent after implementing any significant feature or code change
- Use `code-refactor-specialist` agent when dealing with legacy code, complex methods, or code that's difficult to maintain
- Use `security-auditor` agent for authentication, packet handling, database queries, or any security-sensitive features

**Infrastructure:**
- Use `windows-automation-specialist` agent for creating/modifying `.bat` files, PowerShell scripts, or Windows automation tasks

**Database:**
- Invoke `Backend Queries` skill when writing SQL queries, ORM operations, or optimizing database performance

**General Practice:**
- Always use agents proactively when their expertise matches the task
- After completing implementation work, use code-reviewer agent before marking tasks complete
- For multi-step complex features, break down work and use appropriate agents for each phase

## Project Overview

This is an L1R (Lineage Remastered) private server project consisting of two main components:
- **L1J-WantedServer**: Java-based game server (L1J fork)
- **LineageWarriorClient**: Game client with launcher and assets

The project is focused on translating a Korean Lineage server to English and adding custom features for private server hosting.

## Server Architecture (L1J-WantedServer)

### Build and Run

**Build the server:**
```cmd
cd "L1J-WantedServer\L1J-Wanted"
ant
```
This compiles Java sources and creates `l1jserver.jar`.

**Run the server:**
```cmd
cd "L1J-WantedServer\L1J-Wanted"
startserver.bat
```

**Database setup:**
```cmd
cd "L1J-WantedServer\L1J-Wanted\db"
import_db.bat
```

### Configuration System

The server uses a modular configuration system managed by `Config.java`, which loads settings from multiple property files in the `config/` directory:

- `server.properties` - Core server settings (DB connection, ports, thread pools)
- `rates.properties` - Game rates (XP, drops, adena)
- `altsettings.properties` - Alternative gameplay settings
- `webserver.properties` - Web server/app center configuration
- Additional config files for specific systems (quest, dungeon, enchant, etc.)

All configuration classes are in `src/l1j/server/configure/` and are loaded during server initialization.

### Core Architecture Layers

**1. Server Entry Point**
- `Server.java` - Main server bootstrapper, initializes Config, Database, GameServer, LoginServer, and WebServer
- `GameServer.java` - Game server initialization, loads all data tables and systems
- `LoginServer.java` - Handles client authentication
- `WebServer.java` - HTTP server for app center (web management interface)

**2. Network Layer**
- `server/clientpackets/` - Incoming packet handlers from client
- `server/serverpackets/` - Outgoing packet handlers to client
- `GameClient.java` - Represents connected client session
- `Opcodes.java` - Packet opcode definitions (multiple versions for different client builds)

**3. Game Systems** (`GameSystem/`)

Major subsystems include:
- `ai/` - NPC AI system with brain-based decision making (AiBrain, actions, area-specific AI)
- `attendance/` - Daily attendance/login rewards
- `beginnerquest/` - Tutorial/beginner quest system
- `craft/` - Item crafting system
- `dungeontimer/` - Instanced dungeon timers
- `huntingquest/` - Hunting quest management
- `smelting/` - Item smelting/upgrading
- `charactertrade/` - Character trading between accounts

**4. Instance Dungeons** (`IndunSystem/`)
- Instance-based dungeon implementations (clan dungeons, boss raids, minigames)
- Each dungeon type has its own package (antqueen, dragonraid, unicorntemple, etc.)

**5. LFC System** (`LFCSystem/`)
- Looking For Clan/Party system for matchmaking

**6. Quest System** (`QuestSystem/`)
- Quest loading, templates, and reward compensation

**7. Robot/Bot System** (`RobotSystem/`)
- Automated AI-controlled player characters

**8. Data Management**
- `server/datatables/` - In-memory caches of database tables (items, NPCs, spawns, etc.)
- `server/templates/` - Template objects for game entities
- `server/storage/` - Persistent storage handlers
- `common/bin/` - Binary file loaders for client data files (.bin format)

**9. Web Interface** (`web/`)
- HTTP dispatcher and REST-like endpoints for the app center
- Handles item shop, character management, rankings, etc.

### Key Data Files

Located in `data/`:
- `desc-e.tbl` / `desc-k.tbl` - English/Korean item/NPC descriptions
- `server-e.tbl` / `server-k.tbl` - Server-side text (dialogs, messages)
- `string-e.tbl` / `string-k.tbl` - UI strings
- `db_*.tbl` - Item, armor, weapon, NPC definitions
- `xml/` - XML configurations for quests, NPCs, spawns, etc.
- `mapcache/` - Precompiled map data

### Translation Workflow

The project is actively translating Korean content to English:
1. Text is stored in dual `.tbl` files (e.g., `desc-e.tbl` for English, `desc-k.tbl` for Korean)
2. Server code references descriptions by ID (e.g., `$123`) which maps to `.tbl` entries
3. Translation tasks tracked in `README.md` roadmap
4. Database tables may contain both Korean and English columns

### Database Schema

Uses MySQL with the following main tables:
- Character data (characters, character_items, character_quests, etc.)
- World data (spawnlist, droplist, shops, etc.)
- Configuration (weapon, armor, etcitem, npc definitions)
- Logs (chat_log, command_log, etc. in LogDB/)

## Client Architecture (LineageWarriorClient)

- `LineageWarrior/` - Main client directory
- `LWLauncher.exe` - Custom launcher for patching and launching
- `.pak` and `.idx` files - Packed game assets (images, sprites, sounds, UI)
- `config/` - Client-side character configurations
- `data/`, `maps/` - Client data files

The launcher handles auto-updates, account creation, and connecting to the server list.

## Common Development Workflows

### Adding a New Game Feature

1. Create necessary tables in database (`db/` SQL scripts)
2. Add configuration properties if needed (`config/*.properties` and corresponding `configure/*.java`)
3. Implement game system in appropriate package (`GameSystem/`, `IndunSystem/`, etc.)
4. Add data tables/loaders in `server/datatables/`
5. Create client/server packets in `clientpackets/`/`serverpackets/`
6. Add commands if needed in `server/command/`
7. Update `.tbl` files for text/descriptions
8. Test with client

### Modifying Server Rates or Settings

1. Edit appropriate config file in `config/` (e.g., `rates.properties`)
2. Restart server (config is loaded on startup)
3. For dynamic changes, find the Config field in code (e.g., `Config.RATE.RATE_XP`) and understand where it's used

### Working with Packets

- Packets use opcodes defined in `Opcodes.java` (different versions for different client builds)
- Client packets extend base classes in `clientpackets/`
- Server packets extend base classes in `serverpackets/`
- Packet structure must match client expectations exactly

### Working with NPCs and AI

- NPC definitions in `data/xml/npc/` and database tables
- AI behaviors in `GameSystem/ai/`
- AI uses brain-based architecture with actions (Attack, Move, PickUp, None)
- Area-specific AI in `GameSystem/ai/area/`

## Important Notes

- **Character Encoding**: Always use UTF-8 for Korean/English text
- **Thread Pools**: Server uses configurable thread pool (default 512, see `server.properties`)
- **Memory**: Server configured with 1GB min, 8GB max heap (`startserver.bat`)
- **Client Versions**: Multiple opcode files exist for different client versions (check `Opcodes_*.java`)
- **Security**: Passwords currently not hashed (SHA encryption is a TODO item)
- **Logging**: Server logs to `log/` directory and database `LogDB/` by date

## Map Format System (Critical Information)

### Server vs Client Maps - Two Independent Systems

Lineage uses **two completely separate map systems** that serve different purposes:

#### Client Maps (.s32 format)
- **Location**: `LineageWarriorClient/map/[mapid]/XXXXYYYY.s32`
- **Purpose**: Visual rendering (graphics, terrain textures, tile images)
- **Format**: Binary compressed format with:
  - Tile dictionaries and reference IDs (16-bit or 32-bit)
  - Compression/encoding (RLE or custom)
  - Graphics metadata and tile attributes
  - File sizes: 50KB-250KB per 64×64 segment
- **Generated by**: Client developers/artists (pre-made, distributed with client)
- **Never changes**: These are static files that ship with the game client
- **Server does NOT generate these**: The L1J server has no code to create .s32 files

#### Server Maps (.txt and .map formats)
- **Location**:
  - Source: `L1J-WantedServer/L1J-Wanted/maps/[mapid].txt`
  - Cache: `L1J-WantedServer/L1J-Wanted/data/mapcache/[mapid].map`
- **Purpose**: Game logic (passability, collision detection, spawns, zones)
- **Format**:
  - `.txt`: Text format, 1 byte per tile (0=walkable, 1+=obstacle)
  - `.map`: Binary cache automatically generated from .txt by server on startup
- **Generated by**: Server automatically creates .map from .txt
- **Server code**: Uses simple byte-per-tile format internally

#### Map Filename Generation for .s32 Files

The correct formula for generating .s32 segment filenames:
```
For a map with:
- Start coordinates: (startX, startY)
- End coordinates: (endX, endY)
- Dimensions: width × height tiles
- Segments: numSegX × numSegY (each 64×64)

Filename coordinates:
fileX = endX - numSegX + 1 + segmentIndexX
fileY = startY + segmentIndexY

Filename: {fileX:x4}{fileY:x4}.s32
```

Example for 200×200 map at world (32768, 32768):
- End coords: (32967, 32967) = (0x80c7, 0x80c7)
- Segments: 4×4 = 16 files
- Files: 80c48000.s32 through 80c78003.s32

#### Important Notes

1. **Maps are independent**: Server .txt changes do NOT require client .s32 updates
2. **For development**: Focus on server .txt format; client already has .s32 files
3. **No auto-generation**: Server does NOT create .s32 files - only .map cache files
4. **Map editor usage**:
   - Edit/export `.txt` files for server logic
   - Client .s32 files remain unchanged (they're just visuals)
   - Server auto-generates `.map` cache on startup
5. **Event maps**: Server's `data/client_maps/` folder contains .s32 files only for special event map distribution/patching

#### Database Map Coordinates

The `mapids` table stores world coordinate boundaries:
```sql
SELECT mapid, startX, endX, startY, endY FROM mapids WHERE mapid = 3;
```

Example (Map 3 - Red Knight Training Camp, 128×128):
- startX=32640, endX=32767, startY=32768, endY=32895

## Project Goals (from README.md)

- Translate all Korean content to English (client files, server messages, database)
- Build custom launcher with auto-patching
- Implement web-based app center for server management
- Create English GM and player commands
- Test all major systems (sieges, quests, PVP, dungeons, etc.)
- always use agents when possible.