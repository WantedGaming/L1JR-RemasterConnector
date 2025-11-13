using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LineageLauncher.Core.Interfaces;
using LineageLauncher.Core.Entities;
using LineageLauncher.App.Views;

namespace LineageLauncher.App.ViewModels;

/// <summary>
/// ViewModel for the login window.
/// Handles user authentication and credential management.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoggingIn;

    [ObservableProperty]
    private bool _isLoginSuccessful;

    public User? AuthenticatedUser { get; private set; }

    public LoginViewModel(IAuthenticationService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Determines if the login button should be enabled.
    /// </summary>
    public bool CanLogin => !string.IsNullOrWhiteSpace(Username)
                           && !string.IsNullOrWhiteSpace(Password)
                           && !IsLoggingIn;

    partial void OnUsernameChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnPasswordChanged(string value) => LoginCommand.NotifyCanExecuteChanged();
    partial void OnIsLoggingInChanged(bool value) => LoginCommand.NotifyCanExecuteChanged();

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoggingIn = true;
        StatusMessage = "Authenticating...";
        IsLoginSuccessful = false;

        try
        {
            AuthenticatedUser = await _authService.AuthenticateAsync(Username, Password);

            if (AuthenticatedUser != null)
            {
                StatusMessage = "Login successful!";
                IsLoginSuccessful = true;

                // Delay to show success message
                await Task.Delay(500);
            }
            else
            {
                StatusMessage = "Invalid username or password.";
                Password = string.Empty; // Clear password on failure
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Login error: {ex.Message}";
            Password = string.Empty;
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    /// <summary>
    /// Opens the account creation dialog.
    /// </summary>
    [RelayCommand]
    private void CreateAccount()
    {
        try
        {
            var createAccountWindow = new CreateAccountWindow
            {
                Owner = Application.Current.MainWindow
            };

            var result = createAccountWindow.ShowDialog();

            if (result == true)
            {
                StatusMessage = "Account created successfully! You can now login.";
                // Optionally pre-fill username from created account
                if (!string.IsNullOrEmpty(createAccountWindow.CreatedUsername))
                {
                    Username = createAccountWindow.CreatedUsername;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to open account creation dialog: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Clears all form data and status messages.
    /// </summary>
    public void Reset()
    {
        Username = string.Empty;
        Password = string.Empty;
        StatusMessage = string.Empty;
        IsLoggingIn = false;
        IsLoginSuccessful = false;
        AuthenticatedUser = null;
    }
}
