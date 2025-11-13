using LineageLauncher.App.Services;
using LineageLauncher.App.ViewModels;
using LineageLauncher.Core.Interfaces;
using LineageLauncher.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LineageLauncher.App;

/// <summary>
/// Extension methods for registering application-specific services and ViewModels.
/// </summary>
public static class AppServiceExtensions
{
    /// <summary>
    /// Registers all ViewModels and application services for the WPF application.
    /// </summary>
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register service implementations
        // Real authentication service (uses /outgame/login endpoint)
        services.AddSingleton<IAuthenticationService, Infrastructure.ConnectorAuthenticationService>();

        // Mock patch service (TODO: Implement real patch service)
        services.AddSingleton<IPatchService, MockPatchService>();

        // NOTE: ILauncherService is already registered in Infrastructure/ServiceCollectionExtensions.cs
        // Do NOT register it again here to avoid DI conflicts!
        // The LinBinLauncher implementation is used by default.

        // Register ViewModels as transient (new instance per request)
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainLauncherViewModel>();

        return services;
    }
}
