using LineageLauncher.Core.Interfaces;
using LineageLauncher.Crypto;
using LineageLauncher.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Infrastructure;

/// <summary>
/// Extension methods for configuring dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all launcher services with the dependency injection container.
    /// </summary>
    public static IServiceCollection AddLauncherServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register Core services
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

        // Register Network services
        services.AddHttpClient<L1JRApiClient>(client =>
        {
            var baseUrl = configuration["Server:ApiBaseUrl"] ?? "http://localhost:8080";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<ConnectorApiClient>(client =>
        {
            var connectorUrl = configuration["Server:ConnectorUrl"] ?? "http://127.0.0.1:8085";
            client.BaseAddress = new Uri(connectorUrl);
            client.Timeout = TimeSpan.FromSeconds(30);

            // Server requires "nc launcher" User-Agent for authentication
            client.DefaultRequestHeaders.Add("User-Agent", "nc launcher");
        });

        // Register Launcher services
        services.AddSingleton<IDllDeploymentService, LineageLauncher.Launcher.DllDeploymentService>();
        services.AddSingleton<ILauncherService, LineageLauncher.Launcher.LinBinLauncher>();

        // Register DLL Injection services (Windows-only)
        if (OperatingSystem.IsWindows())
        {
            services.AddTransient<LineageLauncher.Launcher.Process.ProcessCreator>();
            services.AddTransient<LineageLauncher.Launcher.Injection.DllInjector>();
            services.AddTransient<LineageLauncher.Launcher.IPC.PipeManager>();
            services.AddTransient<LineageLauncher.Launcher.Orchestration.ProcessLaunchOrchestrator>();
        }

        // Register Windows-only services
        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<LineageLauncher.Launcher.HardwareIdCollector>();
        }

        // Register Logging (Console + Debug + File via Serilog)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);

            // Add Serilog file logging
            builder.AddFile("D:\\L1R Project\\l1r-customlauncher\\launcher.log", minimumLevel: LogLevel.Debug);
        });

        return services;
    }
}
