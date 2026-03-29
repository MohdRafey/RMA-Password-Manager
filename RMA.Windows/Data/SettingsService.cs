using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RMA.Windows.Data
{
  public class SettingsService
  {
    private string GetRmaFolderPath()
    {
      string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string rmaFolder = Path.Combine(appData, "RMA");

      if (!Directory.Exists(rmaFolder))
      {
        Directory.CreateDirectory(rmaFolder);
      }

      return rmaFolder;
    }

    public void SaveSalt(byte[] salt, string vaultName)
    {
      string saltPath = Path.Combine(GetRmaFolderPath(), $"{vaultName}.salt");
      File.WriteAllBytes(saltPath, salt);
    }

    public byte[]? LoadSalt(string vaultName)
    {
      string saltPath = Path.Combine(GetRmaFolderPath(), $"{vaultName}.salt");
      return File.Exists(saltPath) ? File.ReadAllBytes(saltPath) : null;
    }

    /// <summary>
    /// Returns true if at least one vault salt exists on the system.
    /// </summary>
    public bool SaltExists()
    {
      return GetAllRegisteredVaultNames().Any();
    }

    /// <summary>
    /// Scans the AppData folder for all .salt files to facilitate the 'Silent Sweep' login.
    /// </summary>
    public List<string> GetAllRegisteredVaultNames()
    {
      string path = GetRmaFolderPath();

      if (!Directory.Exists(path)) return new List<string>();

      return Directory.GetFiles(path, "*.salt")
                      .Select(Path.GetFileNameWithoutExtension)
                      .Where(name => name != null) // Filter out nulls
                      .Cast<string>()              // Cast to non-nullable string
                      .ToList();
    }
  }
}