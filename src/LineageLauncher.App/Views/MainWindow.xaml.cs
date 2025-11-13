using System;
using System.Windows;
using LineageLauncher.App.ViewModels;

namespace LineageLauncher.App.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainLauncherViewModel _viewModel;

    public MainWindow(MainLauncherViewModel viewModel)
    {
        InitializeComponent();

        System.Diagnostics.Debug.WriteLine("[DIAGNOSTIC] MainWindow constructor - InitializeComponent complete");

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        // Log DataContext info
        if (DataContext is MainLauncherViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine("[DIAGNOSTIC] DataContext is MainLauncherViewModel");
            System.Diagnostics.Debug.WriteLine($"[DIAGNOSTIC] ViewModel StartGameCommand: {vm.StartGameCommand != null}");

            // Subscribe to property changes
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.IsGameReady))
                {
                    System.Diagnostics.Debug.WriteLine($"[DIAGNOSTIC] PropertyChanged event - IsGameReady: {vm.IsGameReady}");
                }
            };
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] DataContext is NOT MainLauncherViewModel! Type: {DataContext?.GetType().FullName ?? "NULL"}");
        }

        // Subscribe to logout event
        _viewModel.LogoutRequested += ViewModel_LogoutRequested;
    }

    private void ViewModel_LogoutRequested(object? sender, EventArgs e)
    {
        // Signal the app to handle logout and show login window
        Dispatcher.Invoke(() =>
        {
            Application.Current.MainWindow = null;
            Close();
        });
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DIAGNOSTIC] LaunchButton_Click event fired!");
        System.Diagnostics.Debug.WriteLine($"[DIAGNOSTIC] StartGameCommand exists: {_viewModel.StartGameCommand != null}");
        System.Diagnostics.Debug.WriteLine($"[DIAGNOSTIC] StartGameCommand.CanExecute: {_viewModel.StartGameCommand?.CanExecute(null)}");

        // Manually execute the command to test
        if (_viewModel.StartGameCommand != null && _viewModel.StartGameCommand.CanExecute(null))
        {
            System.Diagnostics.Debug.WriteLine("[DIAGNOSTIC] Manually executing StartGameCommand...");
            _viewModel.StartGameCommand.Execute(null);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[ERROR] StartGameCommand is null or cannot execute!");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Cleanup
        _viewModel.LogoutRequested -= ViewModel_LogoutRequested;
        base.OnClosed(e);
    }
}
