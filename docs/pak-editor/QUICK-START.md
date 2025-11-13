# L1RXmlEditor Quick Start Guide

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 (17.8+) or JetBrains Rider 2023.3+
- Git for version control

## Project Setup

### 1. Clone/Navigate to Repository

```cmd
cd "D:\L1R Project\L1R-PAK-Editor"
```

### 2. Restore NuGet Packages

```cmd
dotnet restore L1RXmlEditor.sln
```

This will download all required packages:
- Avalonia 11.0.6 (UI framework)
- ReactiveUI 19.5.31 (MVVM framework)
- AvaloniaEdit 11.0.6 (XML text editor)
- Microsoft.Extensions.* (DI and configuration)

### 3. Build the Solution

```cmd
dotnet build L1RXmlEditor.sln --configuration Debug
```

### 4. Run the Application

```cmd
dotnet run --project src\L1RXmlEditor.App\L1RXmlEditor.App.csproj
```

## Solution Structure

```
L1R-PAK-Editor/
├── src/
│   ├── L1RXmlEditor.App/              # Avalonia UI application
│   │   ├── L1RXmlEditor.App.csproj
│   │   ├── FodyWeavers.xml            # ReactiveUI IL weaving config
│   │   ├── app.manifest                # Windows DPI awareness
│   │   └── Assets/                     # Images, icons, resources
│   │
│   ├── L1RXmlEditor.Core/             # Business logic
│   │   ├── L1RXmlEditor.Core.csproj
│   │   ├── Models/                     # Domain models
│   │   ├── Services/                   # Service interfaces
│   │   └── ViewModels/                 # MVVM view models
│   │
│   ├── L1RXmlEditor.Infrastructure/   # Data access
│   │   ├── L1RXmlEditor.Infrastructure.csproj
│   │   ├── Xml/                        # XML parsing
│   │   ├── IO/                         # File operations
│   │   └── Validation/                 # XML validation
│   │
│   └── L1RXmlEditor.Plugins/          # Plugin system
│       ├── L1RXmlEditor.Plugins.csproj
│       └── PluginLoader/               # Dynamic plugin loading
│
├── L1RXmlEditor.sln                    # Solution file
├── Directory.Build.props               # Shared MSBuild properties
├── NuGet.config                        # NuGet configuration
├── NUGET-PACKAGES.md                   # Package documentation
└── README.md                           # Project overview
```

## Development Workflow

### Opening in Visual Studio

1. Open `L1RXmlEditor.sln` in Visual Studio 2022
2. Wait for NuGet package restoration (automatic)
3. Build solution (Ctrl+Shift+B)
4. Set `L1RXmlEditor.App` as startup project
5. Press F5 to run with debugger

### Opening in JetBrains Rider

1. Open `L1RXmlEditor.sln` in Rider
2. Wait for NuGet package restoration (automatic)
3. Build solution (Ctrl+Shift+F9)
4. Select `L1RXmlEditor.App` run configuration
5. Click Run button or press Shift+F10

### Command-Line Development

```cmd
# Restore packages
dotnet restore

# Build all projects
dotnet build

# Build specific project
dotnet build src\L1RXmlEditor.Core\L1RXmlEditor.Core.csproj

# Run application
dotnet run --project src\L1RXmlEditor.App\L1RXmlEditor.App.csproj

# Clean build artifacts
dotnet clean

# Run tests (when added)
dotnet test
```

## Key Technologies

### Avalonia UI (11.0.6)
- Cross-platform XAML-based UI framework
- Similar to WPF but runs on Windows, macOS, and Linux
- Uses Fluent Design theme for modern appearance

### ReactiveUI (19.5.31)
- MVVM framework built on Reactive Extensions
- Eliminates boilerplate code using Fody IL weaving
- Observable properties and reactive commands

### AvaloniaEdit (11.0.6)
- Advanced text editor control
- Syntax highlighting for XML
- Line numbers, code folding, search/replace

### Microsoft.Extensions.*
- Dependency injection container
- Configuration system (JSON, environment variables)
- Structured logging framework

## Common Tasks

### Adding a New Package

1. Edit `Directory.Build.props`
2. Add to `<ItemGroup Label="Package Versions">`:
   ```xml
   <PackageVersion Include="PackageName" Version="1.0.0" />
   ```
3. Edit the specific `.csproj` file
4. Add to appropriate `<ItemGroup>`:
   ```xml
   <PackageReference Include="PackageName" />
   ```
5. Restore packages: `dotnet restore`

### Creating a New ViewModel

```csharp
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace L1RXmlEditor.Core.ViewModels;

public class MyViewModel : ReactiveObject
{
    // ReactiveUI.Fody will auto-generate INotifyPropertyChanged
    [Reactive]
    public string Title { get; set; } = "My View";

    [Reactive]
    public bool IsLoading { get; set; }

    public MyViewModel()
    {
        // Initialize reactive commands
    }
}
```

### Creating a New View

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:L1RXmlEditor.Core.ViewModels"
             x:Class="L1RXmlEditor.App.Views.MyView"
             x:DataType="vm:MyViewModel">

    <TextBlock Text="{Binding Title}" />

</UserControl>
```

### Registering Services

In `L1RXmlEditor.App/App.axaml.cs`:

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // Register ViewModels
    services.AddTransient<MainViewModel>();

    // Register Services
    services.AddSingleton<IXmlParserService, XmlParserService>();
    services.AddSingleton<IFileService, FileService>();
}
```

## Troubleshooting

### "Package not found" errors

```cmd
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages again
dotnet restore
```

### Fody weaving errors

1. Verify `FodyWeavers.xml` exists in `L1RXmlEditor.App/`
2. Ensure `ReactiveUI.Fody` package is referenced
3. Clean and rebuild:
   ```cmd
   dotnet clean
   dotnet build
   ```

### Avalonia designer not working

1. Install Avalonia for Visual Studio extension
2. Ensure all Avalonia packages are version 11.0.6
3. Restart Visual Studio/Rider

### Build errors after package updates

1. Check `Directory.Build.props` for version consistency
2. Ensure related packages have matching versions (e.g., all Avalonia.* packages)
3. Delete `bin/` and `obj/` folders, then rebuild

## Next Steps

1. Read `NUGET-PACKAGES.md` for detailed package documentation
2. Review `README.md` for project architecture
3. Explore `src/L1RXmlEditor.Core/` for domain models
4. Check Avalonia documentation for UI development
5. Learn ReactiveUI for MVVM patterns

## Useful Commands

```cmd
# Check for outdated packages
dotnet list package --outdated

# Check for vulnerable packages
dotnet list package --vulnerable

# Update specific package (in Directory.Build.props)
# Edit version, then:
dotnet restore

# Generate package reference report
dotnet list package --include-transitive

# Clean everything
dotnet clean
rmdir /s /q bin obj packages
```

## Documentation Links

- [Avalonia Documentation](https://docs.avaloniaui.net/)
- [ReactiveUI Documentation](https://www.reactiveui.net/)
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [C# 12 Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12)

---

**Ready to start developing!**

For questions or issues, refer to:
- `NUGET-PACKAGES.md` - Package details and troubleshooting
- `README.md` - Project overview and architecture
- GitHub Issues - Bug reports and feature requests
