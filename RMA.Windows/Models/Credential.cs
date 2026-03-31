public class Credential
{
  public string Id { get; set; } = string.Empty;
  public string ServiceName { get; set; } = string.Empty;
  public string? ServiceUrl { get; set; }  // Nullable
  public string? Username { get; set; }    // Nullable
  public string Password { get; set; } = string.Empty;
  public string? Tag { get; set; }         // Nullable
  public string? Notes { get; set; }       // Nullable
  public string? UpdatedAt { get; set; }
  public string? UpdatedBy { get; set; }
}