using CommunityToolkit.Mvvm.ComponentModel;

public partial class Credential : ObservableObject
{
  public string Id { get; set; } = string.Empty;
  public string ServiceName { get; set; } = string.Empty;
  public string? ServiceUrl { get; set; }  // Nullable
  public string? Username { get; set; }    // Nullable
  public string Password { get; set; } = string.Empty;
  public string? Tag { get; set; }         // Nullable
  public string? Notes { get; set; }       // Nullable
  public string? CreatedAt { get; set; }
  public string? UpdatedAt { get; set; }
  public string? UpdatedBy { get; set; }

  [ObservableProperty]
  private bool _isPasswordVisible;

  [ObservableProperty]
  private string _decryptedPassword = "••••••••••••";
}