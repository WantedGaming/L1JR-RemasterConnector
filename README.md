# L1R Custom Launcher

A modern, high-performance game launcher for Lineage Remastered (L1R) private server, built with .NET 8 and C#.

## Overview

This custom launcher provides a professional, production-ready solution for launching the Lineage client and connecting to the L1JR-Server. Built with modern .NET 8.0, WPF, and Clean Architecture principles.

### Key Features
- **Fast startup** (~1-2 seconds vs 3-5 seconds for CEF launchers)
- **Lightweight** (~15MB vs 40MB)
- **Modern security** (Argon2id password hashing, HTTPS)
- **Efficient patching** (parallel downloads, Zstandard compression)
- **Clean architecture** (maintainable, testable, extensible)
- **Modern UI** (WPF with ModernWPF, WebView2 browser)
- **Server integration** (L1JR-Server authentication and API)

## Technology Stack

- **Framework:** .NET 8.0 (LTS)
- **Language:** C# 12
- **UI:** WPF with ModernWPF
- **Architecture:** Clean Architecture + MVVM
- **Browser:** Microsoft Edge WebView2
- **Security:** Argon2id password hashing

## Project Structure

```
L1R-CustomLauncher/
├── docs/                          # Documentation
├── src/                           # Source code
│   ├── LineageLauncher.App/       # WPF Application (Main EXE)
│   ├── LineageLauncher.Core/      # Domain models & interfaces
│   ├── LineageLauncher.Crypto/    # Encryption & security DLL
│   ├── LineageLauncher.Network/   # HTTP API client DLL
│   ├── LineageLauncher.Patcher/   # File patching engine DLL
│   ├── LineageLauncher.Launcher/  # Process manager DLL
│   └── LineageLauncher.Infrastructure/ # Infrastructure services
├── tests/                         # Automated tests
│   ├── LineageLauncher.UnitTests/
│   └── LineageLauncher.IntegrationTests/
├── tools/                         # Development tools
│   ├── PatchManifestGenerator/
│   └── ServerIntegration/
├── README.md                      # This file
└── CHANGELOG.md                   # Version history
```

## Prerequisites

- **Windows 10/11** (64-bit)
- **.NET 8.0 SDK** (or use self-contained build)
- **Visual Studio 2022** or **JetBrains Rider** (recommended IDEs)
- **L1JR-Server** (for server integration)
- **L1R-Client** (Lineage client with Lin.bin)

## Quick Start

### 1. Installation

```bash
cd "D:\L1R Project\L1R-CustomLauncher"

# Restore dependencies
dotnet restore

# Build solution
dotnet build --configuration Release

# Run tests
dotnet test
```

### 2. Development

```bash
# Run the launcher in development mode
cd src/LineageLauncher.App
dotnet run

# Or open in Visual Studio/Rider and press F5
```

### 3. Publishing

```bash
# Create self-contained executable
dotnet publish src/LineageLauncher.App -c Release -r win-x64 --self-contained true
```

## Development Workflow

This project follows the same disciplined workflow as l1r-database:

### Mandatory Steps for Every Task:

1. **Read CHANGELOG.md** - Understand recent changes and context
2. **Create TODO list** - Break task into subtasks, track progress
3. **Use specialized agents** - backend-development agents for C#/.NET
4. **Complete the task** - Write code, test, verify
5. **Update CHANGELOG.md** - Document what was done

### Available Agents

- **backend-architect** - For system architecture and design decisions
- **graphql-architect** - If implementing GraphQL API (optional)
- **tdd-orchestrator** - For test-driven development
- **code-reviewer** - For code quality assurance (use proactively)

## Architecture

The launcher follows **Clean Architecture** principles:

### Layer Structure:
1. **Core** (Domain layer) - Business entities, interfaces, domain logic
2. **Infrastructure** - External concerns (file system, network, crypto)
3. **Application** - WPF UI and ViewModels (MVVM)

### Key Components:

- **LineageLauncher.Core** - Domain models, interfaces, contracts
- **LineageLauncher.Crypto** - XOR encryption, Argon2id password hashing
- **LineageLauncher.Network** - HTTP API client for L1JR-Server
- **LineageLauncher.Patcher** - File patching engine with parallel downloads
- **LineageLauncher.Launcher** - Process manager for Lin.bin execution
- **LineageLauncher.Infrastructure** - Concrete implementations
- **LineageLauncher.App** - WPF UI, ViewModels, MVVM

## Features

### Phase 1: Foundation (Current)
- [ ] .NET 8.0 solution setup
- [ ] Core DLL projects with Clean Architecture
- [ ] Basic authentication
- [ ] Argon2id password hashing
- [ ] HTTP API client

### Phase 2: Patching
- [ ] File patcher engine
- [ ] Parallel downloads
- [ ] Zstandard compression
- [ ] Progress tracking
- [ ] Rollback capability

### Phase 3: Integration
- [ ] L1JR-Server authentication API
- [ ] Server list API
- [ ] Patch manifest API
- [ ] News/announcements API

### Phase 4: UI & Polish
- [ ] WPF main window with ModernWPF
- [ ] WebView2 browser integration
- [ ] Settings panel
- [ ] Auto-update system
- [ ] Discord Rich Presence
- [ ] Logging and error reporting

## Server Integration

The launcher integrates with L1JR-Server through HTTP APIs:

### Endpoints (to be implemented on server):
- `POST /api/auth/login` - User authentication
- `GET /api/patch/manifest` - Get patch list
- `GET /api/patch/download/:file` - Download patch file
- `GET /api/servers` - Get server list
- `GET /api/news` - Get announcements

### Client Launch Process:
1. User enters credentials → Authenticate with server
2. Check for updates → Download patch manifest
3. Apply patches → Download and install files
4. Launch client → Execute Lin.bin with parameters
5. Monitor process → Handle crashes/errors

## Security

### Password Security:
- **Never store plaintext passwords**
- Use **Argon2id** for password hashing (industry standard)
- Secure memory handling for sensitive data

### Communication:
- HTTPS only for server communication
- Certificate validation
- API token authentication

### File Integrity:
- SHA256 checksums for patch files
- Verify file integrity before applying
- Secure file permissions

## Testing

### Unit Tests
- Test business logic in Core
- Test crypto operations
- Test data validation

### Integration Tests
- Test HTTP API communication
- Test file patching process
- Test launcher process management

### Manual Testing
- Test with actual L1JR-Server
- Test with real Lin.bin client
- Test on different Windows versions

## Configuration

Settings stored in `launcher.config.json`:

```json
{
  "ServerUrl": "https://l1r-server.com",
  "ClientPath": "D:\\L1R Project\\L1R-Client\\bin64\\Lin.bin",
  "PatchDirectory": "patches",
  "EnableAutoUpdate": true,
  "LogLevel": "Info"
}
```

## Troubleshooting

### Common Issues

**Issue: .NET 8.0 SDK not found**
- Install from: https://dotnet.microsoft.com/download/dotnet/8.0

**Issue: WPF designer not working**
- Use Visual Studio 2022 (Rider has limited WPF designer support)

**Issue: Lin.bin won't launch**
- Check ClientPath in configuration
- Verify Lin.bin exists and has execute permissions
- Check server connection

**Issue: Build errors**
- Run `dotnet restore`
- Clean and rebuild: `dotnet clean && dotnet build`

## Documentation

- **README.md** - This file (project overview and setup)
- **CHANGELOG.md** - Complete history of all changes
- See existing launcher docs in `D:\L1R Project\docs\launcher\` for reference

## Development Status

**Current Version:** 0.1.0-alpha
**Status:** Initial Setup Phase
**Started:** November 10, 2025

See [CHANGELOG.md](CHANGELOG.md) for detailed progress and updates.

## Contributing

This project follows strict development rules:

1. **ALWAYS read CHANGELOG.md before starting**
2. **ALWAYS create TODO list for each task**
3. **ALWAYS use backend-development agents**
4. **ALWAYS update CHANGELOG.md after completion**
5. **Keep code clean** - remove test/temp files
6. **Two docs only** - README.md and CHANGELOG.md

## License

Part of the L1R private server project.

## Related Projects

- **L1JR-Server** - Java-based game server
- **L1R-Client** - Lineage game client
- **l1r-database** - Next.js web interface for database management
- **L1R-PAK-Tools** - Tools for PAK file manipulation

---

For detailed changes and updates, see [CHANGELOG.md](CHANGELOG.md)
