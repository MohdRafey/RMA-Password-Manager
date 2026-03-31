using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RMA.Windows.Data;
using RMA.Windows.Models;
using System.Collections.ObjectModel;

namespace RMA.Windows.ViewModels
{
  public partial class AddCredentialViewModel : ObservableObject
  {
    public event Action? OnSaveSuccess;

    [ObservableProperty]
    private List<ServiceTemplate> _allTemplates = new();

    [ObservableProperty]
    private string _serviceName = "";

    [ObservableProperty]
    private string _serviceUrl = "";

    [ObservableProperty]
    private string _tag = "";

    [ObservableProperty]
    private string? _notes = "";

    [ObservableProperty]
    private string _username = "";

    public AddCredentialViewModel()
    {
      LoadTemplates();
    }

    private void LoadTemplates()
    {
      // Just load the data once
      AllTemplates = DatabaseService.Instance.GetAllTemplates() ?? new List<ServiceTemplate>();
    }

    partial void OnServiceNameChanged(string value)
    {
      // We still keep this just for the Auto-fill logic
      var exactMatch = AllTemplates.FirstOrDefault(t =>
          t.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

      if (exactMatch != null)
      {
        ServiceUrl = exactMatch.DefaultUrl ?? "";
        Tag = exactMatch.Category ?? "";
      }
    }

    [RelayCommand]
    private void Save(object parameter)
    {
      string updatedBy = "Windows Desktop";

      if (parameter is Wpf.Ui.Controls.PasswordBox pb)
      {
        string rawPassword = pb.Password;
        if (string.IsNullOrEmpty(ServiceName) || string.IsNullOrEmpty(rawPassword)) return;

        byte[] key = VaultService.Instance.GetActiveKey();
        var crypto = new CryptoService();
        string encryptedPassword = crypto.Encrypt(rawPassword, key);

        DatabaseService.Instance.AddCredential(
            ServiceName,
            ServiceUrl,
            Username,
            encryptedPassword,
            Tag,
            Notes,
            updatedBy);
        DatabaseService.Instance.LearnService(ServiceName, ServiceUrl, Tag);

        OnSaveSuccess?.Invoke();
      }
    }
  }
}