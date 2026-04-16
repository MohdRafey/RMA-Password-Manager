using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Data;
using RMA.Windows.Validator;
using RMA.Windows.Views;
using System;
using System;
using System.IO;
using System.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ui = Wpf.Ui.Controls;
using System.Windows.Threading;


namespace RMA.Windows.ViewModels
{
  public partial class LoginViewModel : ObservableObject
  {
    private readonly CryptoService _crypto = new();
    private readonly SettingsService _settings = new();
    private readonly DispatcherTimer _lockoutTimer;

    // --- UI State Properties ---
    [ObservableProperty] private bool _isSetupMode = false;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanLogin))] 
    private bool _isAuthenticating;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanLogin))] 
    private bool _isLockedOut;

    [ObservableProperty] private int _lockoutSeconds;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _idHasError;
    [ObservableProperty] private bool _pinHasError;
    [ObservableProperty] private string _userIdInput = string.Empty;

    [ObservableProperty] private string _setupVaultName = string.Empty;
    [ObservableProperty] private string _setupPin = string.Empty;
    [ObservableProperty] private string _setupConfirmPin = string.Empty;

    [ObservableProperty] private bool _setupNameError;
    [ObservableProperty] private bool _setupPinError;
    [ObservableProperty] private bool _setupConfirmError;

    [ObservableProperty] private string _setupStrengthText = "Weak";
    [ObservableProperty] private string _setupStrengthColor = "#D00000"; // Red

    private int _failedAttempts = 0;
    public bool CanLogin => !IsAuthenticating && !IsLockedOut;

    public LoginViewModel()
    {
      _lockoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      _lockoutTimer.Tick += LockoutTimer_Tick;
    }

    partial void OnUserIdInputChanged(string value)
    {
      IdHasError = false; // Border turns normal immediately
      if (!PinHasError) ErrorMessage = string.Empty;
    }

    partial void OnSetupVaultNameChanged(string value) => SetupNameError = false;
    partial void OnSetupPinChanged(string value)
    {
      SetupPinError = false;
      UpdateStrength(value);
    }

    partial void OnSetupConfirmPinChanged(string value) => SetupConfirmError = false;

    private void UpdateStrength(string pin)
    {
      if (string.IsNullOrEmpty(pin)) { SetupStrengthText = "Empty"; return; }

      // Simple logic for 6-digit PIN strength
      bool isSequential = "01234567890".Contains(pin) || "9876543210".Contains(pin);
      bool isRepeated = pin.All(c => c == pin[0]);

      if (pin.Length < 6) { SetupStrengthText = "Too Short"; SetupStrengthColor = "#D00000"; }
      else if (isSequential || isRepeated) { SetupStrengthText = "Predictable"; SetupStrengthColor = "#E67E22"; }
      else { SetupStrengthText = "Strong"; SetupStrengthColor = "#3A5A40"; }
    }

    [RelayCommand]
    private void ToggleSetup() => IsSetupMode = !IsSetupMode;

    [RelayCommand]
    private async Task AuthenticateAsync(object parameter)
    {
      // Clear previous state
      IdHasError = false;
      PinHasError = false;
      ErrorMessage = string.Empty;

      // Get values from UI (logic to extract PIN from PasswordBox)
      string userId = UserIdInput;
      string pin = ExtractPin(parameter);

      // 1. CALL THE VALIDATOR
      var result = LoginValidator.ValidateLogin(userId, pin);

      if (!result.IsSuccess)
      {
        IdHasError = result.IsIdInvalid;
        PinHasError = result.IsPinInvalid;
        ErrorMessage = result.Message;
        TriggerShake();
        return;
      }

      // 2. PROCEED IF VALID
      IsAuthenticating = true;

      await Task.Delay(1500);

      try
      {
        var allVaults = _settings.GetAllRegisteredVaultNames();
        string? actualVaultName = allVaults.FirstOrDefault(v =>
            v.Equals(userId, StringComparison.OrdinalIgnoreCase));

        if (actualVaultName == null)
        {
          HandleFailedAttempt();
          return;
        }

        byte[]? salt = _settings.LoadSalt(actualVaultName);
        if (salt == null) { HandleFailedAttempt(); return; }

        // Attempt decryption
        byte[] key = _crypto.DeriveKey(pin, salt);
        VaultService.Instance.InitializeVault(key, actualVaultName);

        // If we reach here, success!
        _failedAttempts = 0;
        NavigateToDashboard();
      }
      catch (Exception)
      {
        ClearPasswordBox(parameter);
        HandleFailedAttempt();
      }
      finally
      {
        IsAuthenticating = false;
      }
    }

    private string ExtractPin(object parameter)
    {
      if (parameter is FrameworkElement element)
      {
        // Search the StackPanel for the PasswordBox named 'LoginPinBox'
        var pb = element.FindName("LoginPinBox") as ui.PasswordBox;
        return pb?.Password ?? string.Empty;
      }
      return string.Empty;
    }

    private void ClearPasswordBox(object parameter)
    {
      if (parameter is FrameworkElement element)
      {
        var pb = element.FindName("LoginPinBox") as ui.PasswordBox;
        if (pb != null) pb.Password = string.Empty;
      }
    }

    private void HandleFailedAttempt()
    {
      _failedAttempts++;
      ErrorMessage = "Invalid User ID or Master PIN.";
      TriggerShake();

      if (_failedAttempts >= 3)
      {
        StartLockout(30);
      }
    }

    private void StartLockout(int seconds)
    {
      IsLockedOut = true;
      LockoutSeconds = seconds;
      ErrorMessage = $"Too many attempts. Locked for {seconds}s.";
      _lockoutTimer.Start();
    }

    private void TriggerShake()
    {
      // We call a method in the View via Application.Current.MainWindow
      if (Application.Current.MainWindow is MainWindow loginWindow)
      {
        // We will define this method in MainWindow.xaml.cs next
        (loginWindow as dynamic).ExecuteShake();
      }
    }

    private void LockoutTimer_Tick(object? sender, EventArgs e)
    {
      LockoutSeconds--;
      if (LockoutSeconds <= 0)
      {
        _lockoutTimer.Stop();
        IsLockedOut = false;
        ErrorMessage = string.Empty;
        _failedAttempts = 0; // Reset after wait
      }
    }

    //[RelayCommand]
    //private void CreateVault(object parameter)
    //{
    //  if (parameter is FrameworkElement container)
    //  {
    //    var nameBox = container.FindName("VaultNameBox") as Wpf.Ui.Controls.TextBox;
    //    var pinBox = container.FindName("SetupPinBox") as Wpf.Ui.Controls.PasswordBox;
    //    var confirmBox = container.FindName("ConfirmPinBox") as Wpf.Ui.Controls.PasswordBox;

    //    if (nameBox == null || pinBox == null || confirmBox == null) return;

    //    string vaultName = nameBox.Text.Trim(); // Preserve "RayVault"
    //    string pin = pinBox.Password;

    //    if (string.IsNullOrEmpty(vaultName) || pin.Length != 6) return;

    //    try
    //    {
    //      // Check for existing vault regardless of case
    //      var existing = _settings.GetAllRegisteredVaultNames();
    //      if (existing.Any(v => v.Equals(vaultName, StringComparison.OrdinalIgnoreCase)))
    //      {
    //        RmaDialog.Error("Identity Error","A vault with this name already exists (case-insensitive).");
    //        return;
    //      }

    //      byte[] salt = _crypto.GenerateSalt();
    //      byte[] masterKey = _crypto.DeriveKey(pin, salt);

    //      // Save using the PRESERVED case "RayVault"
    //      _settings.SaveSalt(salt, vaultName);
    //      VaultService.Instance.InitializeVault(masterKey, vaultName);
    //      DatabaseService.Instance.InitializeDatabase();

    //      RmaDialog.Info($"Vault '{vaultName}' created!", "Success");
    //      IsSetupMode = false;
    //    }
    //    catch (Exception ex)
    //    {
    //      RmaDialog.Error($"Exception Error:", "{ex.Message}");
    //    }
    //  }
    //}
    [RelayCommand]
    private void CreateVault()
    {
      // 1. Reset all error states first
      SetupNameError = false;
      SetupPinError = false;
      SetupConfirmError = false;
      ErrorMessage = string.Empty;

      bool hasError = false;

      // 2. Check Vault Name
      if (string.IsNullOrWhiteSpace(SetupVaultName))
      {
        SetupNameError = true;
        hasError = true;
      }

      // 3. Check PIN (and Confirm PIN)
      if (string.IsNullOrEmpty(SetupPin) || SetupPin.Length != 6)
      {
        SetupPinError = true;
        hasError = true;
      }

      if (string.IsNullOrEmpty(SetupConfirmPin) ||
              SetupConfirmPin.Length != 6 ||
              SetupPin != SetupConfirmPin)
      {
        SetupConfirmError = true;
        hasError = true;
      }

      // 4. If any errors were found, trigger feedback and stop
      if (hasError)
      {
        ErrorMessage = "Please correct the highlighted fields.";
        TriggerShake();
        return;
      }

      // 3. Execution
      try
      {
        var existing = _settings.GetAllRegisteredVaultNames();
        if (existing.Any(v => v.Equals(SetupVaultName, StringComparison.OrdinalIgnoreCase)))
        {
          SetupNameError = true;
          ErrorMessage = "Vault name already exists.";
          TriggerShake();
          return;
        }

        byte[] salt = _crypto.GenerateSalt();
        byte[] masterKey = _crypto.DeriveKey(SetupPin, salt);

        _settings.SaveSalt(salt, SetupVaultName);
        VaultService.Instance.InitializeVault(masterKey, SetupVaultName);
        DatabaseService.Instance.InitializeDatabase();

        RmaDialog.Info($"Vault '{SetupVaultName}' created!", "Success");

        // Reset fields and return to login
        SetupVaultName = string.Empty;
        IsSetupMode = false;
      }
      catch (Exception ex)
      {
        RmaDialog.Error("System Error", ex.Message);
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