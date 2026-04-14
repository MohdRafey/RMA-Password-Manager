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

    [ObservableProperty]
    private int _id = 0;

    [ObservableProperty]
    private string _actionButtonText = "Create Entry";

    [ObservableProperty]
    private string _headerText = "Add Credential";

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
      // CRITICAL: If Id > 0, we are editing. Do NOT let the template 
      // auto-fill overwrite our existing database data.
      if (Id > 0) return;

      var exactMatch = AllTemplates.FirstOrDefault(t =>
          t.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

      if (exactMatch != null)
      {
        ServiceUrl = exactMatch.DefaultUrl ?? "";
        Tag = exactMatch.Category ?? "";
      }
    }

    public void LoadCredential(Credential model)
    {
      Id = model.Id;
      // Set ServiceName first
      ServiceName = model.ServiceName;
      ServiceUrl = model.ServiceUrl;
      Username = model.Username;
      Tag = model.Tag;
      Notes = model.Notes;

      ActionButtonText = "Update Entry";
      HeaderText = "Edit Credential";
    }


    [RelayCommand]
    private void Save(object parameter)
    {
      string updatedBy = Environment.MachineName; // More dynamic than hardcoded string

      if (parameter is Wpf.Ui.Controls.PasswordBox pb)
      {
        string rawPassword = pb.Password;
        if (string.IsNullOrEmpty(ServiceName)) return;

        // If editing and password is left blank, you might want to keep the old one.
        // For now, let's assume password is required for both.
        if (string.IsNullOrEmpty(rawPassword) && Id == 0) return;

        byte[] key = VaultService.Instance.GetActiveKey();
        var crypto = new CryptoService();
        string encryptedPassword = crypto.Encrypt(rawPassword, key);

        if (Id > 0)
        {
          // UPDATE EXISTING
          DatabaseService.Instance.UpdateCredential(
              Id,
              ServiceName,
              ServiceUrl,
              Username,
              encryptedPassword,
              Tag,
              Notes,
              updatedBy);
        }
        else
        {
          // CREATE NEW
          DatabaseService.Instance.AddCredential(
              ServiceName,
              ServiceUrl,
              Username,
              encryptedPassword,
              Tag,
              Notes,
              updatedBy);

          DatabaseService.Instance.LearnService(ServiceName, ServiceUrl, Tag);
        }

        OnSaveSuccess?.Invoke();
      }
    }
  }
}