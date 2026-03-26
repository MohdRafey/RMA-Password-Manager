using Microsoft.Data.Sqlite;
using System.IO;

namespace RMA.Windows.Data
{
  public class VaultService
  {
    private const string DbFileName = "vault.rma";
    private string _connectionString;

    public void InitializeVault(byte[] key, string vaultName = "default")
    {
      string hexKey = System.Convert.ToHexString(key);

      // Build path to AppData/Local/RMA
      string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
      string rmaFolder = System.IO.Path.Combine(appData, "RMA");

      if (!System.IO.Directory.Exists(rmaFolder))
        System.IO.Directory.CreateDirectory(rmaFolder);

      string fullPath = System.IO.Path.Combine(rmaFolder, $"{vaultName}.rma");

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

    public bool CheckIfVaultExists() => File.Exists(DbFileName);
  }
}

