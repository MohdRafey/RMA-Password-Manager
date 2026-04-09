using CommunityToolkit.Mvvm.ComponentModel;

public partial class Credential : ObservableObject
{
  // Primary Database Identity
  public string Id { get; set; } = string.Empty;

  // --- Versioning & Ledger Properties ---
  public int GroupId { get; set; }           // The 'Family ID' that stays the same for all versions
  public int Version { get; set; }           // 1, 2, 3... increments on every edit
  public bool IsArchived { get; set; }       // 0 for Live, 1 for History
  public bool IsDeleted { get; set; }        // 1 for Trash

  // --- Core Data ---
  public string ServiceName { get; set; } = string.Empty;
  public string? ServiceUrl { get; set; }
  public string? Username { get; set; }
  public string Password { get; set; } = string.Empty; // Encrypted Base64 string
  public string? Tag { get; set; }
  public string? Notes { get; set; }

  // --- Audit Metadata ---
  public string? CreatedAt { get; set; }
  public string? UpdatedAt { get; set; }
  public string? UpdatedBy { get; set; }
  public string? DeletedAt { get; set; }     // NEW: Timestamp for the Recycle Bin

  // --- UI State Properties ---
  [ObservableProperty]
  private bool _isPasswordVisible;

  [ObservableProperty]
  private string _decryptedPassword = "••••••••••••";
}