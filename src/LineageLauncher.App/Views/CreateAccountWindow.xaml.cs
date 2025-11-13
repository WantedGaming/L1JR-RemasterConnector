using System;
using System.Windows;
using LineageLauncher.Crypto;
using LineageLauncher.Launcher;
using LineageLauncher.Network;
using Microsoft.Extensions.DependencyInjection;

namespace LineageLauncher.App.Views;

/// <summary>
/// Interaction logic for CreateAccountWindow.xaml
/// </summary>
public partial class CreateAccountWindow : Window
{
    private readonly ConnectorApiClient _apiClient;
    private readonly HardwareIdCollector? _hardwareIdCollector;

    public string CreatedUsername { get; private set; } = string.Empty;

    public CreateAccountWindow()
    {
        InitializeComponent();

        // Get services from DI container
        _apiClient = ((App)Application.Current).ServiceProvider.GetRequiredService<ConnectorApiClient>();
        _hardwareIdCollector = ((App)Application.Current).ServiceProvider.GetService<HardwareIdCollector>();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate inputs
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;
            var phone = PhoneTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                StatusTextBlock.Text = "Please enter a username.";
                return;
            }

            if (username.Length > 40)
            {
                StatusTextBlock.Text = "Username must be 40 characters or less.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                StatusTextBlock.Text = "Please enter a password.";
                return;
            }

            if (password.Length < 6 || password.Length > 20)
            {
                StatusTextBlock.Text = "Password must be 6-20 characters.";
                return;
            }

            // Validate password has both letters and digits
            bool hasLetter = false;
            bool hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsLetter(c)) hasLetter = true;
                if (char.IsDigit(c)) hasDigit = true;
            }

            if (!hasLetter || !hasDigit)
            {
                StatusTextBlock.Text = "Password must contain both letters and numbers.";
                return;
            }

            if (password != confirmPassword)
            {
                StatusTextBlock.Text = "Passwords do not match.";
                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                phone = "-";
            }

            // Disable button during creation
            CreateButton.IsEnabled = false;
            StatusTextBlock.Text = "Creating account...";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Gray;

            // Collect hardware IDs
            string macAddress = string.Empty;
            string hddId = string.Empty;
            string boardId = string.Empty;
            string nicId = string.Empty;

            if (_hardwareIdCollector != null && OperatingSystem.IsWindows())
            {
#pragma warning disable CA1416
                macAddress = _hardwareIdCollector.GetMacAddress();
                hddId = _hardwareIdCollector.GetHardDriveId();
                boardId = _hardwareIdCollector.GetMotherboardId();
                nicId = _hardwareIdCollector.GetNetworkInterfaceId();
#pragma warning restore CA1416
            }

            // Calculate HMAC
            const string createAccountPath = "/outgame/accountcreate";
            var macInfo = HmacCalculator.CalculateMacInfo(hddId, macAddress, createAccountPath);

            // Call API
            var resultCode = await _apiClient.CreateAccountAsync(
                username,
                password,
                phone,
                macAddress,
                hddId,
                boardId,
                nicId,
                macInfo);

            if (resultCode == "SUCCESS")
            {
                CreatedUsername = username;
                MessageBox.Show(
                    $"Account '{username}' created successfully! You can now login.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                // Show error from server
                StatusTextBlock.Text = $"Account creation failed: {resultCode}";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                CreateButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Error: {ex.Message}";
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            CreateButton.IsEnabled = true;
        }
    }
}
