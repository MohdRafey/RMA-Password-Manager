using System.IO;

namespace RMA.Windows.Data
{
  public class SettingsService
  {
    private const string SaltFileName = "vault.salt";

    /// <summary>
    /// Saves the cryptographic salt to a local file.
    /// </summary>
    public void SaveSalt(byte[] salt)
    {
      File.WriteAllBytes(SaltFileName, salt);
    }

    /// <summary>
    /// Loads the salt from the local file. Returns null if the file doesn't exist.
    /// </summary>
    public byte[]? LoadSalt()
    {
      if (File.Exists(SaltFileName))
      {
        return File.ReadAllBytes(SaltFileName);
      }

      return null;
    }

    /// <summary>
    /// Checks if the salt file exists (useful for detecting first-run status).
    /// </summary>
    public bool SaltExists() => File.Exists(SaltFileName);
  }
}