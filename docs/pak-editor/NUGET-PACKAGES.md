# NuGet Package Configuration Guide

## Overview

This document details the NuGet package dependencies for the L1RXmlEditor Avalonia application solution.

## Solution Structure

The solution uses a clean architecture approach with four projects:

1. **L1RXmlEditor.App** - Avalonia UI presentation layer
2. **L1RXmlEditor.Core** - Business logic and domain models
3. **L1RXmlEditor.Infrastructure** - XML parsing, file I/O, validation
4. **L1RXmlEditor.Plugins** - Plugin system implementations

## Centralized Package Management

The solution uses **Directory.Build.props** for centralized version management. All package versions are defined once in the root Directory.Build.props file using `<PackageVersion>` elements, ensuring consistency across all projects.

### Benefits:
- Single source of truth for package versions
- Easy version updates (change in one place)
- Consistent versions across all projects
- Shared MSBuild properties (LangVersion, Nullable, etc.)

## Project-Specific Packages

### L1RXmlEditor.App (Presentation Layer)

**Purpose:** Main executable, Avalonia UI, MVVM implementation

**Package References:**

```xml
<!-- Avalonia UI Framework -->
<PackageReference Include="Avalonia" Version="11.0.6" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.6" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.6" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.6" />
<PackageReference Include="AvaloniaEdit" Version="11.0.6" />

<!-- ReactiveUI for MVVM -->
<PackageReference Include="ReactiveUI" Version="19.5.31" />
<PackageReference Include="ReactiveUI.Fody" Version="19.5.31" />

<!-- Dependency Injection -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
```

**Key Features:**
- **Avalonia 11.0.6**: Cross-platform UI framework (Windows, macOS, Linux)
- **Avalonia.Desktop**: Desktop-specific implementations and native dialogs
- **Avalonia.Themes.Fluent**: Modern Fluent Design theme
- **Avalonia.ReactiveUI**: Integration between Avalonia and ReactiveUI
- **AvaloniaEdit**: Advanced text editor control for XML editing
- **ReactiveUI.Fody**: IL weaving for automatic property notifications (eliminates boilerplate)

**Special Configuration:**
- Requires `FodyWeavers.xml` for ReactiveUI.Fody weaving
- `app.manifest` for Windows DPI awareness and compatibility

---

### L1RXmlEditor.Core (Business Logic Layer)

**Purpose:** Domain models, business rules, service interfaces

**Package References:**

```xml
<!-- ReactiveUI for Observable Properties -->
<PackageReference Include="ReactiveUI" Version="19.5.31" />

<!-- Collections -->
<PackageReference Include="System.Collections.Immutable" Version="8.0.0" />

<!-- Dependency Injection Abstractions -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

**Key Features:**
- **ReactiveUI**: Enables observable properties and reactive command patterns in ViewModels
- **System.Collections.Immutable**: Immutable collections for thread-safe, functional domain models
- **Abstractions packages**: Interface-only packages for loose coupling (no concrete implementations)

**Design Principles:**
- No concrete infrastructure dependencies
- Only abstractions and interfaces
- Immutable domain models using records and immutable collections

---

### L1RXmlEditor.Infrastructure (Data Access Layer)

**Purpose:** XML parsing, file operations, validation, external integrations

**Package References:**

```xml
<!-- Collections -->
<PackageReference Include="System.Collections.Immutable" Version="8.0.0" />

<!-- Configuration -->
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

**Built-in Framework Types (No Package Reference Needed):**
- `System.Xml.Linq` - XML parsing and manipulation
- `System.IO` - File system operations
- `System.Text.Json` - JSON serialization

**Key Features:**
- **Microsoft.Extensions.Configuration**: Flexible configuration system
- **Configuration.Json**: JSON configuration file support
- **System.Collections.Immutable**: Immutable data structures for parsed XML data

**Implementation Notes:**
- XML parsing uses LINQ to XML (System.Xml.Linq)
- File I/O uses async APIs (FileStream, StreamReader/Writer)
- Validation uses XSD schemas or custom validators

---

### L1RXmlEditor.Plugins (Plugin System)

**Purpose:** Plugin infrastructure, dynamic loading, extensibility

**Package References:**

```xml
<!-- Collections -->
<PackageReference Include="System.Collections.Immutable" Version="8.0.0" />

<!-- Dependency Injection -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

**Key Features:**
- **System.Collections.Immutable**: Plugin registry and metadata storage
- **DI Abstractions**: Plugin service registration and discovery

**Plugin Architecture:**
- Dynamic plugin discovery using reflection
- Plugin metadata using custom attributes
- Service registration via IServiceCollection extensions

---

## Project References

```
L1RXmlEditor.App
├── L1RXmlEditor.Core
├── L1RXmlEditor.Infrastructure
└── L1RXmlEditor.Plugins

L1RXmlEditor.Infrastructure
└── L1RXmlEditor.Core

L1RXmlEditor.Plugins
└── L1RXmlEditor.Core
```

**Dependency Flow:**
- Core has no dependencies (domain-centric)
- Infrastructure depends on Core (implements Core interfaces)
- Plugins depends on Core (extends Core abstractions)
- App depends on all three (composition root)

---

## Additional Configuration Files

### 1. Directory.Build.props

**Location:** Solution root

**Purpose:**
- Centralized package version management
- Shared MSBuild properties
- Common build configuration

**Key Properties:**
```xml
<TargetFramework>net8.0</TargetFramework>
<LangVersion>12.0</LangVersion>
<Nullable>enable</Nullable>
<TieredCompilation>true</TieredCompilation>
```

### 2. NuGet.config

**Location:** Solution root

**Purpose:**
- Package source configuration
- Global packages folder location
- Package source mapping

**Configuration:**
```xml
<packageSources>
  <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
</packageSources>
<config>
  <add key="globalPackagesFolder" value="packages" />
</config>
```

### 3. FodyWeavers.xml

**Location:** L1RXmlEditor.App project

**Purpose:**
- Configure Fody IL weaving
- Enable ReactiveUI property notifications

**Configuration:**
```xml
<Weavers>
  <ReactiveUI />
</Weavers>
```

### 4. app.manifest

**Location:** L1RXmlEditor.App project

**Purpose:**
- Windows application manifest
- DPI awareness configuration
- OS compatibility declarations

---

## Modern C# Features Enabled

### Language Version: C# 12

**Enabled Features:**
- Primary constructors
- Collection expressions
- Inline arrays
- Lambda expression improvements
- Raw string literals
- Required members
- File-scoped namespaces
- Global using directives
- Nullable reference types

### Nullable Reference Types

**Enabled:** `<Nullable>enable</Nullable>`

**Benefits:**
- Compile-time null safety
- Explicit nullable annotations (`string?`)
- Warnings for potential null reference exceptions
- Better code quality and fewer runtime errors

### Performance Optimizations

**Tiered Compilation:**
```xml
<TieredCompilation>true</TieredCompilation>
<TieredCompilationQuickJit>true</TieredCompilationQuickJit>
```

**Benefits:**
- Faster startup time
- Better runtime performance
- JIT optimization tiers

---

## Package Restore and Build

### Restore NuGet Packages

```cmd
dotnet restore L1RXmlEditor.sln
```

### Build Solution

```cmd
dotnet build L1RXmlEditor.sln --configuration Release
```

### Run Application

```cmd
dotnet run --project src\L1RXmlEditor.App\L1RXmlEditor.App.csproj
```

---

## Package Update Strategy

### Check for Updates

```cmd
dotnet list package --outdated
```

### Update Specific Package

Edit the version in `Directory.Build.props`:

```xml
<PackageVersion Include="Avalonia" Version="11.0.7" />
```

Then restore:

```cmd
dotnet restore
```

### Update All Packages

Update version numbers in `Directory.Build.props` for all `<PackageVersion>` elements, then restore.

---

## Troubleshooting

### Common Issues

1. **Package Not Found**
   - Verify NuGet.config points to correct sources
   - Check internet connectivity
   - Clear NuGet cache: `dotnet nuget locals all --clear`

2. **Version Conflicts**
   - All versions are centralized in Directory.Build.props
   - Ensure no individual projects override versions
   - Check for transitive dependency conflicts

3. **Fody Weaving Errors**
   - Verify FodyWeavers.xml is in App project
   - Ensure ReactiveUI.Fody package is referenced
   - Clean and rebuild: `dotnet clean && dotnet build`

4. **Avalonia Designer Issues**
   - Ensure Avalonia version is consistent across all packages
   - Install Avalonia for Visual Studio extension
   - Restart Visual Studio/Rider

---

## Package Compatibility Matrix

| Package | Version | .NET Version | Notes |
|---------|---------|--------------|-------|
| Avalonia | 11.0.6 | .NET 6+ | LTS version |
| AvaloniaEdit | 11.0.6 | .NET 6+ | Must match Avalonia version |
| ReactiveUI | 19.5.31 | .NET 6+ | Latest stable |
| ReactiveUI.Fody | 19.5.31 | .NET 6+ | Must match ReactiveUI version |
| Microsoft.Extensions.* | 8.0.0 | .NET 8+ | Matches .NET 8.0 SDK |
| System.Collections.Immutable | 8.0.0 | .NET 8+ | Framework version |

---

## Best Practices

1. **Version Management**
   - Always update versions in Directory.Build.props only
   - Keep related packages at same version (e.g., all Avalonia packages)
   - Test after updating major versions

2. **Package Selection**
   - Prefer Microsoft.Extensions.* abstractions in Core/Infrastructure
   - Use concrete implementations only in App project
   - Minimize third-party dependencies in Core layer

3. **Performance**
   - Use System.Collections.Immutable for thread-safe collections
   - Prefer abstractions (interfaces) for testability
   - Enable nullable reference types for compile-time safety

4. **Security**
   - Keep packages up to date for security patches
   - Review package vulnerabilities: `dotnet list package --vulnerable`
   - Use only trusted package sources

---

## Additional Resources

- [Avalonia Documentation](https://docs.avaloniaui.net/)
- [ReactiveUI Documentation](https://www.reactiveui.net/)
- [AvaloniaEdit Documentation](https://github.com/AvaloniaUI/AvaloniaEdit)
- [.NET Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [C# 12 Features](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12)

---

**Document Version:** 1.0
**Last Updated:** 2025-11-09
**Target Framework:** .NET 8.0
**C# Version:** 12.0
