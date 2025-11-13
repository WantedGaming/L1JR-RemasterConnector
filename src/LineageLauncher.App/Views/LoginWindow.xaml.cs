using System;
using System.Windows;
using System.Windows.Controls;
using LineageLauncher.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LineageLauncher.App.Views;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        // Subscribe to property changes to handle password binding
        PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;

        // Subscribe to ViewModel events
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Focus username field on load
        Loaded += (s, e) => UsernameTextBox.Focus();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Update ViewModel password when PasswordBox changes
        // Note: PasswordBox.Password cannot be bound directly for security reasons
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.Password = passwordBox.Password;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Handle successful login
        if (e.PropertyName == nameof(LoginViewModel.IsLoginSuccessful) && _viewModel.IsLoginSuccessful)
        {
            // Dispatch to UI thread and close window
            Dispatcher.Invoke(() =>
            {
                DialogResult = true;
                Close();
            });
        }

        // Clear password box when password property is cleared
        if (e.PropertyName == nameof(LoginViewModel.Password) && string.IsNullOrEmpty(_viewModel.Password))
        {
            Dispatcher.Invoke(() => PasswordBox.Clear());
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Cleanup
        PasswordBox.PasswordChanged -= PasswordBox_PasswordChanged;
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        base.OnClosed(e);
    }
}
