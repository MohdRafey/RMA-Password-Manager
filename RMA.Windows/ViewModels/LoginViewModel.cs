using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Data;
using System;
using System.IO;
using System.Linq;
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

    [RelayCommand]
    private void Authenticate(object parameter)
    {
      if (parameter is FrameworkElement container)
      {
        var nameBox = container.FindName("LoginUserIdBox") as Wpf.Ui.Controls.TextBox;
        var passwordBox = container.FindName("LoginPinBox") as Wpf.Ui.Controls.PasswordBox;

        if (nameBox == null || passwordBox == null) return;

        string inputName = nameBox.Text.Trim();
        string pin = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(inputName) || string.IsNullOrWhiteSpace(pin)) return;

        try
        {
          // 1. FIND THE ACTUAL FILENAME
          // We look through registered vaults to find a case-insensitive match
          var allVaults = _settings.GetAllRegisteredVaultNames();
          string? actualVaultName = allVaults.FirstOrDefault(v =>
              v.Equals(inputName, StringComparison.OrdinalIgnoreCase));

          if (actualVaultName == null)
          {
            MessageBox.Show("Access Denied: Vault not found.");
            return;
          }

          // 2. PROCEED WITH ACTUAL CASE
          byte[]? salt = _settings.LoadSalt(actualVaultName);
          if (salt == null) return;

          byte[] key = _crypto.DeriveKey(pin, salt);
          VaultService.Instance.InitializeVault(key, actualVaultName);

          // Navigation logic...
          NavigateToDashboard();
        }
        catch (Exception)
        {
          passwordBox.Password = string.Empty;
          MessageBox.Show("Access Denied: Incorrect PIN.");
        }
      }
    }

    [RelayCommand]
    private void CreateVault(object parameter)
    {
      if (parameter is FrameworkElement container)
      {
        var nameBox = container.FindName("VaultNameBox") as Wpf.Ui.Controls.TextBox;
        var pinBox = container.FindName("SetupPinBox") as Wpf.Ui.Controls.PasswordBox;
        var confirmBox = container.FindName("ConfirmPinBox") as Wpf.Ui.Controls.PasswordBox;

        if (nameBox == null || pinBox == null || confirmBox == null) return;

        string vaultName = nameBox.Text.Trim(); // Preserve "RayVault"
        string pin = pinBox.Password;

        if (string.IsNullOrEmpty(vaultName) || pin.Length != 6) return;

        try
        {
          // Check for existing vault regardless of case
          var existing = _settings.GetAllRegisteredVaultNames();
          if (existing.Any(v => v.Equals(vaultName, StringComparison.OrdinalIgnoreCase)))
          {
            MessageBox.Show("A vault with this name already exists (case-insensitive).");
            return;
          }

          byte[] salt = _crypto.GenerateSalt();
          byte[] masterKey = _crypto.DeriveKey(pin, salt);

          // Save using the PRESERVED case "RayVault"
          _settings.SaveSalt(salt, vaultName);
          VaultService.Instance.InitializeVault(masterKey, vaultName);
          DatabaseService.Instance.InitializeDatabase();

          MessageBox.Show($"Vault '{vaultName}' created!", "Success");
          IsSetupMode = false;
        }
        catch (Exception ex)
        {
          MessageBox.Show($"Error: {ex.Message}");
        }
      }
    }

    private void NavigateToDashboard()
    {
      var dashboard = new RMA.Windows.Views.DashboardWindow();
      Application.Current.MainWindow = dashboard;
      dashboard.Show();

      foreach (Window window in Application.Current.Windows)
      {
        if (window is MainWindow) { window.Close(); break; }
      }
    }
  }
}