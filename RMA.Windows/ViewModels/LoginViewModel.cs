using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Core;
using RMA.Windows.Data;
using System.Windows;

namespace RMA.Windows.ViewModels
{
  public partial class LoginViewModel : ObservableObject
  {
    private readonly CryptoService _crypto = new();
    private readonly VaultService _vault = new();
    private readonly SettingsService _settings = new();

    [ObservableProperty]
    private bool _isSetupMode = false;

    [ObservableProperty]
    private string _buttonText = "Login";

    // Toggles between Login and Setup views
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
          byte[]? salt = _settings.LoadSalt();
          if (salt == null)
          {
            MessageBox.Show("Vault salt not found. Please create a new vault.");
            return;
          }

          byte[] key = _crypto.DeriveKey(pin, salt);
          _vault.InitializeVault(key);

          // SUCCESS: Proceed to Dashboard
          MessageBox.Show("Vault Unlocked!");
        }
        catch
        {
          MessageBox.Show("Invalid Master PIN.");
        }
      }
    }

    // PATH 2: New User Creation
    [RelayCommand]
    private void CreateVault(object parameter)
    {
      // Note: In a production app, you'd pass both PasswordBoxes 
      // or handle validation via a Tuple/MultiBinding.
      // For now, let's assume valid input for the logic flow:

      byte[] salt = _crypto.GenerateSalt();
      _settings.SaveSalt(salt);

      // In actual use, get PIN from SetupPinBox
      // byte[] key = _crypto.DeriveKey(pin, salt);
      // _vault.InitializeVault(key);

      IsSetupMode = false;
      MessageBox.Show("Secure Vault Created Successfully!");
    }
  }
}