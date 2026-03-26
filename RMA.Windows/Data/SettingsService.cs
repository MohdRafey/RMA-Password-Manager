using System.IO;

namespace RMA.Windows.Data
{
  public class SettingsService
  {
    private const string SaltFileName = "vault.salt";

    public void SaveSalt(byte[] salt, string vaultName)
    {
      string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string rmaFolder = Path.Combine(appData, "RMA");

      // Ensure directory exists
      Directory.CreateDirectory(rmaFolder);

      string saltPath = Path.Combine(rmaFolder, $"{vaultName}.salt");
      File.WriteAllBytes(saltPath, salt);
    }

    public byte[]? LoadSalt(string vaultName)
    {
      string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string saltPath = Path.Combine(appData, "RMA", $"{vaultName}.salt");

      if (!File.Exists(saltPath)) return null;
      return File.ReadAllBytes(saltPath);
    }

    /// <summary>
    /// Checks if the salt file exists (useful for detecting first-run status).
    /// </summary>
    public bool SaltExists() => File.Exists(SaltFileName);
  }
}