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
    private string _serviceUrl = "";

    [ObservableProperty]
    private string _tag = "";

    [ObservableProperty]
    private ObservableCollection<ServiceTemplate> _filteredTemplates = new();

    [ObservableProperty]
    private string _serviceName = "";

    [ObservableProperty]
    private string _username = "";

    // This handles the filtering automatically when you type
    partial void OnServiceNameChanged(string value)
    {
      string query = value?.ToLower() ?? "";

      // Filter the list
      var matches = AllTemplates
          .Where(t => t.Name.ToLower().Contains(query))
          .ToList();

      // Clear and refill the ObservableCollection
      FilteredTemplates.Clear();
      foreach (var match in matches)
      {
        FilteredTemplates.Add(match);
      }

      // Handle auto-fill for exact matches
      var exactMatch = AllTemplates.FirstOrDefault(t =>
          t.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

      if (exactMatch != null)
      {
        ServiceUrl = exactMatch.DefaultUrl ?? "";
        Tag = exactMatch.Category ?? "";
      }
    }

    public AddCredentialViewModel()
    {
      LoadTemplates();
    }

    private void LoadTemplates()
    {
      var data = DatabaseService.Instance.GetAllTemplates() ?? new List<ServiceTemplate>();
      AllTemplates = data;

      FilteredTemplates.Clear();
      foreach (var item in data) FilteredTemplates.Add(item);
    }

    [RelayCommand]
    private void Save(object parameter)
    {
      if (parameter is Wpf.Ui.Controls.PasswordBox pb)
      {
        string rawPassword = pb.Password;
        if (string.IsNullOrEmpty(ServiceName) || string.IsNullOrEmpty(rawPassword)) return;

        byte[] key = VaultService.Instance.GetActiveKey();
        var crypto = new CryptoService();
        string encryptedPassword = crypto.Encrypt(rawPassword, key);

        DatabaseService.Instance.AddCredential(ServiceName, ServiceUrl, Username, encryptedPassword, Tag);
        DatabaseService.Instance.LearnService(ServiceName, ServiceUrl, Tag);

        OnSaveSuccess?.Invoke();
      }
    }
  }
}