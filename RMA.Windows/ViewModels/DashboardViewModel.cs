using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Data;
using RMA.Windows.Models;
using RMA.Windows.Views;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;

namespace RMA.Windows.ViewModels
{
  public partial class DashboardViewModel : ObservableObject
  {
    private readonly CryptoService _crypto = new();
    private readonly SettingsService _settings = new();

    [ObservableProperty]
    private string _vaultName;

    [ObservableProperty]
    private ObservableCollection<Credential> _credentials = new();

    public bool IsVaultEmpty => Credentials.Count == 0;

    public DashboardViewModel()
    {
      // Set initial vault name from the Singleton
      VaultName = VaultService.Instance.ActiveVaultName ?? "Vault";
      LoadCredentials();
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
        // Tip: You can trigger a WPF UI Snackbar here
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

      // 1. Initialize Window
      var editWindow = new AddCredentialWindow();

      // Set the owner so it centers correctly over the main dashboard
      editWindow.Owner = System.Windows.Application.Current.MainWindow;

      // 2. Use the new helper method you just wrote
      // This handles: Loading text to VM, Decrypting Password, and Setting the PasswordBox
      editWindow.PrepareForEdit(credential);

      // 3. Optional: Auto-close the window on success
      if (editWindow.DataContext is AddCredentialViewModel vm)
      {
        vm.OnSaveSuccess += () => editWindow.Close();
      }

      // 4. Show Window as a modal dialog
      editWindow.ShowDialog();

      // 5. Refresh the dashboard list to show the updated entry
      LoadCredentials();
    }

    [RelayCommand]
    private void DeleteCredential(Credential credential)
    {
      if (credential == null) return;

      // Add your DatabaseService.Instance.Delete logic here
      // Then refresh the list
    }
  }
}