# Changelog

All notable changes to the L1R Custom Launcher project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project structure created
- Created folder hierarchy for .NET solution
  - `src/` - Source code directory with 7 project folders
  - `tests/` - Test projects (Unit and Integration)
  - `tools/` - Development tools (PatchManifestGenerator, ServerIntegration)
  - `docs/` - Documentation directory
- CHANGELOG.md file to track all project changes
- README.md with project overview and quick start guide
- Comprehensive documentation files in `docs/`:
  - ARCHITECTURE.md - System architecture and design patterns
  - ENCRYPTION.md - Security implementation and encryption details
  - PATCHING.md - Complete patching system design and implementation (updated with file organization section)
  - SERVER-INTEGRATION.md - L1J server integration guide with code examples
  - DLL-REQUIREMENTS.md - Complete DLL dependency reference
- PROJECT-STATUS.md - Real-time project progress tracking
- Updated main L1R Project README.md with L1R-CustomLauncher section

### Project Structure
```
L1R-CustomLauncher/
├── docs/
├── src/
│   ├── LineageLauncher.App/
│   ├── LineageLauncher.Core/
│   ├── LineageLauncher.Crypto/
│   ├── LineageLauncher.Infrastructure/
│   ├── LineageLauncher.Launcher/
│   ├── LineageLauncher.Network/
│   └── LineageLauncher.Patcher/
├── tests/
│   ├── LineageLauncher.IntegrationTests/
│   └── LineageLauncher.UnitTests/
└── tools/
    ├── PatchManifestGenerator/
    └── ServerIntegration/
```

### Changed
- Moved project location from `D:\L1R-CustomLauncher` to `D:\L1R Project\L1R-CustomLauncher` for better organization alongside other L1R components
- Enhanced PATCHING.md with comprehensive "Client Directory Structure & File Organization" section:
  - Complete LineageWarriorClient directory tree
  - File type locations and patch targets (executables, translations, PAK files, maps, configs, DLLs)
  - Manifest examples for each file type
  - Priority system (1=critical to 10=most common)
  - Files that should NEVER be patched
  - Typical patch sizes by type
  - Directory creation rules
  - Backup location recommendations
  - File path validation for security

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

---

## Version History

## [0.1.0] - 2025-11-09

### Added
- Project initialization
- Basic folder structure
- CHANGELOG.md for version tracking

---

## Categories Explanation

- **Added**: New features or functionality
- **Changed**: Changes to existing functionality
- **Deprecated**: Features that will be removed in future versions
- **Removed**: Features that have been removed
- **Fixed**: Bug fixes
- **Security**: Security improvements or vulnerability fixes

## Semantic Versioning

- **MAJOR** version (X.0.0): Incompatible API changes
- **MINOR** version (0.X.0): New functionality in a backwards compatible manner
- **PATCH** version (0.0.X): Backwards compatible bug fixes

---

**Project Started:** November 9, 2025
**Current Version:** 0.1.0-alpha
**Status:** In Development
