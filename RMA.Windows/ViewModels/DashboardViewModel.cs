using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Core;
using RMA.Windows.Data;
using System;
using System.Windows;

namespace RMA.Windows.ViewModels
{
  public partial class DashboardViewModel : ObservableObject
  {
    private readonly CryptoService _crypto = new();
    private readonly SettingsService _settings = new();

    [ObservableProperty]
    private string _vaultName;

    public DashboardViewModel()
    {
      // Set initial vault name from the Singleton
      VaultName = VaultService.Instance.ActiveVaultName ?? "Vault";
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
  }
}