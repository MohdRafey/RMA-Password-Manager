using Microsoft.Data.Sqlite;
using System.IO;

namespace RMA.Windows.Data
{
  public class VaultService
  {
    private const string DbFileName = "vault.rma";
    private string _connectionString;

    public void InitializeVault(byte[] key)
    {
      // Convert the 32-byte key to a hex string for SQLCipher
      string hexKey = System.Convert.ToHexString(key);
      _connectionString = $"Data Source={DbFileName};Password={hexKey};";

      using (var connection = new SqliteConnection(_connectionString))
      {
        connection.Open();

        // Create the table for passwords if it doesn't exist
        var command = connection.CreateCommand();
        command.CommandText =
        @"
                    CREATE TABLE IF NOT EXISTS Credentials (
                        Id GUID PRIMARY KEY,
                        Title TEXT NOT NULL,
                        Username TEXT,
                        Password TEXT,
                        Url TEXT,
                        Notes TEXT,
                        LastModified DATETIME
                    );
                ";
        command.ExecuteNonQuery();
      }
    }

    public bool CheckIfVaultExists() => File.Exists(DbFileName);
  }
}