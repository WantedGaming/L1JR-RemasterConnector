# L1R Custom Launcher

A modern, high-performance game launcher for Lineage Remastered (L1R) private server, built with .NET 8 and C#.

## Overview

This custom launcher replaces the existing `LWLauncher.exe` (40MB CEF-based launcher) with a lightweight, modern alternative.

### Key Benefits
- **Fast startup** (~1-2 seconds vs 3-5 seconds)
- **Smaller footprint** (~15MB vs 40MB)
- **Modern security** (Argon2id password hashing, HTTPS)
- **Efficient patching** (parallel downloads, Zstandard compression)
- **Clean architecture** (maintainable, testable, extensible)
- **Modern UI** (Windows 11 Fluent Design)

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
├── tools/                         # Development tools
└── CHANGELOG.md                   # Version history
```

## Technology Stack

- **Framework:** .NET 8.0 (LTS)
- **Language:** C# 12
- **UI:** WPF with ModernWPF
- **Architecture:** Clean Architecture + MVVM
- **Browser:** Microsoft Edge WebView2

## Quick Start

### Prerequisites

- Windows 10/11 (64-bit)
- .NET 8.0 SDK (or use self-contained build)
- L1J-WantedServer (for server integration)

### Building

```bash
cd "D:\L1R Project\L1R-CustomLauncher"

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run tests
dotnet test
```

## Documentation

See the `docs/` folder for comprehensive documentation on:
- Architecture and design
- Encryption and security
- Patching system
- Server integration
- Build and deployment

## Status

**Version:** 0.1.0-alpha
**Status:** In Development
**Started:** November 9, 2025

See [CHANGELOG.md](CHANGELOG.md) for version history and updates.

## Development Roadmap

### Phase 1: Foundation (Weeks 1-3)
- [ ] .NET solution setup
- [ ] Core DLL projects
- [ ] Basic authentication
- [ ] Password hashing
- [ ] HTTP API client

### Phase 2: Patching (Weeks 4-6)
- [ ] File patcher engine
- [ ] Parallel downloads
- [ ] Compression support
- [ ] Progress tracking

### Phase 3: Integration (Weeks 7-8)
- [ ] L1J server endpoints
- [ ] Server authentication
- [ ] Server list API

### Phase 4: Polish (Weeks 9-11)
- [ ] WPF UI
- [ ] WebView2 browser
- [ ] Settings panel
- [ ] Auto-update

## License

Part of the L1R private server project.

---

For detailed changes, see [CHANGELOG.md](CHANGELOG.md)
