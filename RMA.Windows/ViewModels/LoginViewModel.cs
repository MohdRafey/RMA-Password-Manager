using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Core;
using RMA.Windows.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace RMA.Windows.ViewModels
{
  public partial class LoginViewModel : ObservableObject
  {
    private readonly CryptoService _crypto = new();
    private readonly SettingsService _settings = new();

    [ObservableProperty]
    private bool _isSetupMode = false;

    [RelayCommand]
    private void ToggleSetup() => IsSetupMode = !IsSetupMode;

    // PATH 1: Existing User Login (The Silent Sweep)
    [RelayCommand]
    private void Authenticate(object parameter)
    {
      if (parameter is Wpf.Ui.Controls.PasswordBox passwordBox)
      {
        string pin = passwordBox.Password;
        if (string.IsNullOrWhiteSpace(pin)) return;

        var vaultNames = _settings.GetAllRegisteredVaultNames();
        bool anyVaultOpened = false;

        try
        {
          foreach (var vaultName in vaultNames)
          {
            byte[]? salt = _settings.LoadSalt(vaultName);
            if (salt == null) continue;

            try
            {
              byte[] key = _crypto.DeriveKey(pin, salt);

              // IMPORTANT: Use the Singleton Instance so the whole app shares the state
              VaultService.Instance.InitializeVault(key, vaultName);

              anyVaultOpened = true;
              break; // Stop once we find the matching vault
            }
            catch
            {
              // Wrong PIN for this specific vault file, try the next one
              continue;
            }
          }

          if (anyVaultOpened)
          {
            var dashboard = new RMA.Windows.Views.DashboardWindow();
            Application.Current.MainWindow = dashboard;
            dashboard.Show();

            foreach (Window window in Application.Current.Windows)
            {
              if (window is MainWindow)
              {
                window.Close();
                break;
              }
            }
          }
          else
          {
            passwordBox.Password = string.Empty;
            MessageBox.Show("Access Denied: Incorrect PIN.");
          }
        }
        catch (Exception)
        {
          MessageBox.Show("A system error occurred during authentication.");
        }
      }
    }

    // PATH 2: New User Creation
    [RelayCommand]
    private void CreateVault(object parameter)
    {
      if (parameter is FrameworkElement container)
      {
        var nameBox = container.FindName("VaultNameBox") as Wpf.Ui.Controls.TextBox;
        var pinBox = container.FindName("SetupPinBox") as Wpf.Ui.Controls.PasswordBox;
        var confirmBox = container.FindName("ConfirmPinBox") as Wpf.Ui.Controls.PasswordBox;

        if (nameBox == null || pinBox == null || confirmBox == null) return;

        string vaultName = nameBox.Text.Trim();
        string pin = pinBox.Password;
        string confirm = confirmBox.Password;

        if (string.IsNullOrEmpty(vaultName) || pin.Length != 6 || pin != confirm)
        {
          MessageBox.Show("Please check your input. PIN must be 6 digits.");
          return;
        }

        try
        {
          byte[] salt = _crypto.GenerateSalt();
          byte[] masterKey = _crypto.DeriveKey(pin, salt);

          _settings.SaveSalt(salt, vaultName);

          // Initialize the Singleton for the new vault
          VaultService.Instance.InitializeVault(masterKey, vaultName);

          MessageBox.Show($"{vaultName} vault created!", "Success");

          IsSetupMode = false;
          // Optional: Navigate to dashboard immediately or let them login
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Error: {ex.Message}");
        }
      }
    }
  }
}