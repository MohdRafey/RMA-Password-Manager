using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Linq; // Required for SequenceEqual

namespace RMA.Windows.Data
{
  public class VaultService
  {
    private static VaultService _instance;
    public static VaultService Instance => _instance ??= new VaultService();

    private string _connectionString;
    private byte[] _activeMasterKey;
    public string ActiveVaultName { get; private set; }

    // --- NEW: VERIFICATION LOGIC ---
    public bool VerifyKey(byte[] providedKey)
    {
      if (_activeMasterKey == null || providedKey == null)
        return false;

      // Securely compare the stored key from login with the one entered on the lock screen
      return Enumerable.SequenceEqual(_activeMasterKey, providedKey);
    }

    public void InitializeVault(byte[] key, string vaultName = "default")
    {
      _activeMasterKey = key;
      ActiveVaultName = vaultName;
      string hexKey = Convert.ToHexString(key);

      string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string rmaFolder = Path.Combine(appData, "RMA");

      if (!Directory.Exists(rmaFolder))
        Directory.CreateDirectory(rmaFolder);

      string fullPath = Path.Combine(rmaFolder, $"{vaultName}.rma");

      // We use the Hex key for the SQLite Encryption (SQLCipher)
      _connectionString = $"Data Source={fullPath};Password={hexKey};";

      using (var connection = new SqliteConnection(_connectionString))
      {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Credentials (
                        Id TEXT PRIMARY KEY, 
                        Title TEXT NOT NULL,
                        Username TEXT,
                        Password TEXT,
                        Url TEXT,
                        Notes TEXT,
                        LastModified TEXT DEFAULT CURRENT_TIMESTAMP
                    );";
        command.ExecuteNonQuery();
      }
    }

    // Updated to check the actual AppData path
    public bool CheckIfVaultExists(string vaultName = "default")
    {
      string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      string fullPath = Path.Combine(appData, "RMA", $"{vaultName}.rma");
      return File.Exists(fullPath);
    }

    // Inside VaultService class
    public byte[] GetActiveKey()
    {
      if (_activeMasterKey == null)
        throw new InvalidOperationException("No active vault session. Key is null.");

      return _activeMasterKey;
    }

    // Also, let's add a way to get the ConnectionString for our DatabaseService
    public string GetConnectionString() => _connectionString;
  }
}