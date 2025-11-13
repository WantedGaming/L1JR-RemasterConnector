using System;
using System.IO;
using System.Windows;
using LineageLauncher.App.ViewModels;
using LineageLauncher.App.Views;
using LineageLauncher.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LineageLauncher.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider ServiceProvider => _host.Services;

    public App()
    {
        // Prevent automatic shutdown when LoginWindow closes
        // Application will only shut down when explicitly called via Shutdown()
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(appDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register infrastructure services
                services.AddLauncherServices(context.Configuration);

                // Register app-specific services and ViewModels
                services.AddAppServices(context.Configuration);

                // Register Windows as transient (new instance each time)
                services.AddTransient<LoginWindow>();
                services.AddTransient<Views.MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Show login window first
        ShowLoginWindow();

        base.OnStartup(e);
    }

    private void ShowLoginWindow()
    {
        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();

        // Handle successful login
        Console.WriteLine("Showing login window...");
        if (loginWindow.ShowDialog() == true)
        {
            Console.WriteLine("Login dialog returned true");
            // Get the authenticated user from the ViewModel
            var loginViewModel = loginWindow.DataContext as LoginViewModel;
            Console.WriteLine($"LoginViewModel: {loginViewModel != null}");
            Console.WriteLine($"AuthenticatedUser: {loginViewModel?.AuthenticatedUser != null}");

            if (loginViewModel?.AuthenticatedUser != null)
            {
                Console.WriteLine($"Showing MainWindow for user: {loginViewModel.AuthenticatedUser.Username}");
                ShowMainWindow(loginViewModel.AuthenticatedUser);
            }
            else
            {
                Console.WriteLine("AuthenticatedUser is null, shutting down");
                // Login cancelled or failed, exit app
                Shutdown();
            }
        }
        else
        {
            Console.WriteLine("Login dialog returned false or was closed");
            // User closed login window, exit app
            Shutdown();
        }
    }

    private void ShowMainWindow(LineageLauncher.Core.Entities.User user)
    {
        try
        {
            Console.WriteLine("Creating MainWindow...");
            var mainWindow = _host.Services.GetRequiredService<Views.MainWindow>();
            Console.WriteLine("MainWindow created successfully");

            var mainViewModel = mainWindow.DataContext as MainLauncherViewModel;
            Console.WriteLine($"MainViewModel: {mainViewModel != null}");

            if (mainViewModel != null)
            {
                Console.WriteLine("Initializing MainViewModel...");
                // Initialize the main window with the authenticated user
                mainViewModel.Initialize(user);
                Console.WriteLine("MainViewModel initialized");
            }

            Console.WriteLine("Setting Application.Current.MainWindow...");
            MainWindow = mainWindow;
            Console.WriteLine($"Application.Current.MainWindow set: {MainWindow != null}");

            Console.WriteLine("Calling mainWindow.Show()...");
            mainWindow.Show();
            Console.WriteLine("mainWindow.Show() completed");

            // Handle logout by showing login window again
            mainWindow.Closed += (s, e) =>
            {
                Console.WriteLine($"MainWindow Closed event fired. Application.Current.MainWindow == null: {MainWindow == null}");
                if (MainWindow == null)
                {
                    // Logout was requested, show login window
                    Console.WriteLine("Logout detected, showing login window again");
                    ShowLoginWindow();
                }
                else
                {
                    // User closed window normally, shut down the application
                    Console.WriteLine("User closed window normally, shutting down");
                    Shutdown();
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in ShowMainWindow: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}
