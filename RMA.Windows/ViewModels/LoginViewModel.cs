using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Core;
using RMA.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace RMA.Windows.ViewModels
{
  public partial class LoginViewModel : ObservableObject
  {
    private readonly CryptoService _crypto = new();
    private readonly VaultService _vault = new();
    private readonly SettingsService _settings = new();

    [ObservableProperty]
    private bool _isSetupMode = false;

    // Command to switch between Login and Setup
    [RelayCommand]
    private void ToggleSetup() => IsSetupMode = !IsSetupMode;

    // PATH 1: Existing User Login
    [RelayCommand]
    private void Authenticate(object parameter)
    {
      if (parameter is Wpf.Ui.Controls.PasswordBox passwordBox)
      {
        string pin = passwordBox.Password;
        if (string.IsNullOrWhiteSpace(pin)) return;

        try
        {
          // 1. Setup the Vault Session
          string currentVaultName = "Ray";
          byte[]? salt = _settings.LoadSalt(currentVaultName);

          if (salt == null)
          {
            MessageBox.Show($"No salt file found for {currentVaultName}.");
            return;
          }

          byte[] key = _crypto.DeriveKey(pin, salt);
          _vault.InitializeVault(key, currentVaultName);

          // 2. Initialize and Show the Dashboard
          var dashboard = new RMA.Windows.Views.DashboardWindow();

          // Set the new window as the app's main window so it doesn't close on us
          System.Windows.Application.Current.MainWindow = (System.Windows.Window)dashboard;

          dashboard.Show();

          // 3. Find and close the Login Window specifically
          // We use a loop to find the 'MainWindow' (Login) to ensure we don't accidentally close the Dashboard we just opened
          foreach (Window window in Application.Current.Windows)
          {
            if (window is MainWindow)
            {
              window.Close();
              break;
            }
          }
        }
        catch (Exception ex)
        {
          // Clear the password box on failure for security
          passwordBox.Password = string.Empty;
          MessageBox.Show("Access Denied: Invalid PIN or Vault Error.");
        }
      }
    }

    // PATH 2: New User Creation (The "Handshake")
    [RelayCommand]
    private void CreateVault(object parameter)
    {
      // The parameter is the StackPanel from XAML
      if (parameter is FrameworkElement container)
      {
        // Find our controls by name
        var nameBox = container.FindName("VaultNameBox") as Wpf.Ui.Controls.TextBox;
        var pinBox = container.FindName("SetupPinBox") as Wpf.Ui.Controls.PasswordBox;
        var confirmBox = container.FindName("ConfirmPinBox") as Wpf.Ui.Controls.PasswordBox;

        if (nameBox == null || pinBox == null || confirmBox == null) return;

        string vaultName = nameBox.Text.Trim();
        string pin = pinBox.Password;
        string confirm = confirmBox.Password;

        // 1. Validations
        if (string.IsNullOrEmpty(vaultName))
        {
          MessageBox.Show("Please give your vault a name.");
          return;
        }

        if (pin.Length != 6 || !int.TryParse(pin, out _))
        {
          MessageBox.Show("PIN must be exactly 6 digits.");
          return;
        }

        if (pin != confirm)
        {
          MessageBox.Show("PINs do not match.");
          return;
        }

        try
        {
          // 2. The Crypto Process
          byte[] salt = _crypto.GenerateSalt();

          // 3. The Stretching (Argon2id)
          byte[] masterKey = _crypto.DeriveKey(pin, salt);

          // 4. Persistence
          // We save the salt so we can derive this key again later
          _settings.SaveSalt(salt, vaultName);

          // Create the encrypted .rma file in AppData
          _vault.InitializeVault(masterKey, vaultName);

          MessageBox.Show($"{vaultName} vault created successfully!", "Sovereign Success");

          // 5. Cleanup UI
          IsSetupMode = false;
          nameBox.Text = string.Empty;
          pinBox.Password = string.Empty;
          confirmBox.Password = string.Empty;
        }
        catch (System.Exception ex)
        {
          MessageBox.Show($"Error creating vault: {ex.Message}");
        }
      }
    }
  }
}