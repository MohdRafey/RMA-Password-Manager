using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace RMA.Windows.Data
{
  public class DatabaseService
  {
    private static DatabaseService? _instance;
    public static DatabaseService Instance => _instance ??= new DatabaseService();

    // Always pull the connection string that contains the encryption key
    private string ConnectionString => VaultService.Instance.GetConnectionString();

    public void InitializeDatabase()
    {
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();
      var command = connection.CreateCommand();

      // 1. Credentials Table
      command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Credentials (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ServiceName TEXT NOT NULL,
                    ServiceUrl TEXT,
                    Username TEXT,
                    Password TEXT NOT NULL,
                    Tag TEXT,
                    CreatedAt DATETIME,
                    UpdatedAt DATETIME
                );
                
                // 2. Services Look-up Table (For the picker)
                CREATE TABLE IF NOT EXISTS ServiceTemplates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE,
                    DefaultUrl TEXT,
                    Category TEXT, -- e.g., 'Social Media', 'Banking'
                    IconPath TEXT
                );";

      command.ExecuteNonQuery();
      SeedServiceTemplates(); // Optional: Fill with defaults like Facebook, Google
    }

    public void AddCredential(string name, string url, string user, string pass, string tag)
    {
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();
      using var command = connection.CreateCommand();

      command.CommandText = @"
                INSERT INTO Credentials (ServiceName, ServiceUrl, Username, Password, Tag, CreatedAt, UpdatedAt) 
                VALUES ($name, $url, $user, $pass, $tag, $date, $date)";

      command.Parameters.AddWithValue("$name", name);
      command.Parameters.AddWithValue("$url", url ?? (object)DBNull.Value);
      command.Parameters.AddWithValue("$user", user ?? (object)DBNull.Value);
      command.Parameters.AddWithValue("$pass", pass);
      command.Parameters.AddWithValue("$tag", tag ?? "General");
      command.Parameters.AddWithValue("$date", DateTime.Now);

      command.ExecuteNonQuery();
    }

    private void SeedServiceTemplates()
    {
      // Logic to insert 'Facebook', 'Google', etc., if the table is empty
    }
  }
}