using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Core;
using RMA.Windows.Data;
using System;

namespace RMA.Windows.ViewModels
{
  public partial class AddCredentialViewModel : ObservableObject
  {
    [ObservableProperty] private string _serviceName = "";
    [ObservableProperty] private string _serviceUrl = "";
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _tag = "";

    public event Action? OnSaveSuccess;

    [RelayCommand]
    private void Save(object parameter)
    {
      if (parameter is Wpf.Ui.Controls.PasswordBox pb)
      {
        string rawPassword = pb.Password;
        if (string.IsNullOrEmpty(ServiceName) || string.IsNullOrEmpty(rawPassword)) return;

        // 1. Get the Key from our Singleton VaultService
        byte[] key = VaultService.Instance.GetActiveKey();

        // 2. Encrypt the password
        var crypto = new CryptoService();
        string encryptedPassword = crypto.Encrypt(rawPassword, key);

        // 3. Save to DB (Database logic below)
        DatabaseService.Instance.AddCredential(
            ServiceName, ServiceUrl, Username, encryptedPassword, Tag);

        OnSaveSuccess?.Invoke();
      }
    }
  }
}