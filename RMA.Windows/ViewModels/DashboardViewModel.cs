using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Data;
using RMA.Windows.Models;
using RMA.Windows.Services;
using RMA.Windows.Views;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace RMA.Windows.ViewModels
{
  public partial class DashboardViewModel : ObservableObject
  {
    public event Action<string, string>? OnRequestNotification;
    private readonly CryptoService _crypto = new();
    private readonly SettingsService _settings = new();
    private INotificationService _notifications;
    private System.Windows.Threading.DispatcherTimer _timer;

    [ObservableProperty]
    private string _vaultName;
    [ObservableProperty]
    private string _statusText = "Vault Secure";
    [ObservableProperty]
    private string _currentTime;

    [ObservableProperty]
    private ObservableCollection<Credential> _credentials = new();

    public bool IsVaultEmpty => Credentials.Count == 0;

    public DashboardViewModel()
    {
      // Set initial vault name from the Singleton
      VaultName = VaultService.Instance.ActiveVaultName ?? "Vault";
      LoadCredentials();

      // Setup the Clock
      _timer = new System.Windows.Threading.DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(1); // Check every second
      _timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("HH:mm");
      _timer.Start();

      // Initial value so it's not empty for the first second
      CurrentTime = DateTime.Now.ToString("HH:mm");
    }

    public void SetNotificationService(INotificationService service)
    {
      _notifications = service;
    }

    public void LoadCredentials()
    {
      // Fetch from Database
      var data = DatabaseService.Instance.GetAllCredentials();

      Credentials.Clear();
      foreach (var item in data)
      {
        Credentials.Add(item);
      }
    }

    [RelayCommand]
    private void AddCredential()
    {
      // 1. Initialize ViewModel and Window
      var vm = new AddCredentialViewModel();
      var win = new AddCredentialWindow
      {
        DataContext = vm,
        Owner = System.Windows.Application.Current.MainWindow
      };

      // 2. Setup the success trigger
      vm.OnSaveSuccess += () => win.DialogResult = true;

      // 3. Show the dialog
      if (win.ShowDialog() == true)
      {
        // 4. Refresh the list
        LoadCredentials();

        // 5. Trigger the Service for both Snackbar and Status Bar
        _notifications?.Notify(
            "Vault Updated",
            $"{vm.ServiceName} has been added successfully.",
            NotificationType.Success
        );
      }
    }

    public bool AttemptUnlock(string pin)
    {
      if (string.IsNullOrWhiteSpace(pin)) return false;

      try
      {
        string activeVault = VaultService.Instance.ActiveVaultName;
        if (string.IsNullOrEmpty(activeVault)) return false;

        byte[]? salt = _settings.LoadSalt(activeVault);
        if (salt == null) return false;

        byte[] enteredKey = _crypto.DeriveKey(pin, salt);

        // Verify against memory
        return VaultService.Instance.VerifyKey(enteredKey);
      }
      catch
      {
        return false;
      }
    }

    // --- COMMANDS ---

    [RelayCommand]
    private void TogglePassword(Credential credential)
    {
      if (credential == null) return;

      // Get the key from your VaultService singleton
      byte[] masterKey = VaultService.Instance.GetActiveKey();

      credential.IsPasswordVisible = !credential.IsPasswordVisible;

      if (credential.IsPasswordVisible)
      {
        // Use the _crypto instance if Decrypt is not static
        credential.DecryptedPassword = _crypto.Decrypt(credential.Password, masterKey);
      }
      else
      {
        credential.DecryptedPassword = "••••••••••••";
      }
    }

    [RelayCommand]
    private void CopyPassword(Credential credential)
    {
      if (credential == null) return;

      byte[] masterKey = VaultService.Instance.GetActiveKey();

      // Decrypt on the fly for the clipboard
      string plainText = _crypto.Decrypt(credential.Password, masterKey);

      if (!string.IsNullOrEmpty(plainText))
      {
        Clipboard.SetText(plainText);

        // Use the service!
        _notifications?.Notify("Clipboard", $"Password for {credential.ServiceName} copied.", NotificationType.Info);
      }
    }

    [RelayCommand]
    private void CopyUsername(Credential credential)
    {
      if (credential?.Username != null)
      {
        Clipboard.SetText(credential.Username);
      }
    }

    [RelayCommand]
    private void EditCredential(Credential credential)
    {
      if (credential == null) return;

      var editWindow = new AddCredentialWindow();
      editWindow.Owner = System.Windows.Application.Current.MainWindow;
      editWindow.PrepareForEdit(credential);

      bool isSaved = false; // Flag to track success

      if (editWindow.DataContext is AddCredentialViewModel vm)
      {
        vm.OnSaveSuccess += () =>
        {
          isSaved = true;
          editWindow.Close();
        };
      }

      editWindow.ShowDialog();

      if (isSaved)
      {
        LoadCredentials();

        // Notify the View to show the Snackbar
        // You can use a custom Action property on the VM or the CommunityToolkit Messenger
        OnRequestNotification?.Invoke("Updated", $"{credential.ServiceName} has been updated.");
      }
    }

    [RelayCommand]
    private void DeleteCredential(Credential credential)
    {
      if (credential == null) return;

      // 1. Use your custom RMA Dialog
      // Title, Message, and showCancel = true (default)
      bool confirmed = RmaDialog.Warn(
          "Move to Trash",
          $"Are you sure you want to move '{credential.ServiceName}' to the Recycle Bin?\nThis will hide all historical versions of this account."
      );

      if (confirmed)
      {
        // 2. Execute the Soft Delete using GroupId
        int versionsAffected = DatabaseService.Instance.DeleteCredential(credential.GroupId);

        if (versionsAffected > 0)
        {
          // 3. Update the UI collection
          LoadCredentials();

          // 4. Update the "Empty Vault" UI state
          OnPropertyChanged(nameof(IsVaultEmpty));

          System.Diagnostics.Debug.WriteLine($"[DB] Soft-deleted GroupId {credential.GroupId}. Total versions hidden: {versionsAffected}");
        }
        else
        {
          RmaDialog.Error("System Error", "The record could not be found in the database.");
        }
      }
    }
  }
}