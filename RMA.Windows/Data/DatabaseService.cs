using Microsoft.Data.Sqlite;
using RMA.Windows.Models;
using System;
using System.Collections.Generic;
using System.Net;

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

      // Use a Transaction to ensure both tables are created 
      // before the connection closes.
      using var transaction = connection.BeginTransaction();
      try
      {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        // Inside DatabaseService.cs -> InitializeDatabase()
        command.CommandText = @"
          CREATE TABLE IF NOT EXISTS Credentials (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ServiceName TEXT NOT NULL,
            ServiceUrl TEXT,
            Username TEXT,
            Password TEXT NOT NULL,
            Tag TEXT,
            Notes TEXT,           -- Added Notes
            CreatedAt DATETIME,
            UpdatedAt DATETIME,
            UpdatedBy TEXT,       -- Added UpdatedBy
            IsDeleted INTEGER DEFAULT 0,
            DeletedDate DATETIME NULL
           );

           CREATE TABLE IF NOT EXISTS ServiceTemplates (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT UNIQUE,
            DefaultUrl TEXT,
            Category TEXT, 
            IconPath TEXT
           );";

        command.ExecuteNonQuery();
        transaction.Commit();

        // Only seed AFTER the transaction is committed
        SeedServiceTemplates();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public void AddCredential(string name, string url, string user, string pass, string tag, string notes, string updatedBy)
    {
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();
      using var command = connection.CreateCommand();

      // 1. Update the SQL to include Notes and UpdatedBy
      command.CommandText = @"
        INSERT INTO Credentials 
        (ServiceName, ServiceUrl, Username, Password, Tag, Notes, UpdatedBy, CreatedAt, UpdatedAt) 
        VALUES 
        ($name, $url, $user, $pass, $tag, $notes, $updatedBy, $date, $date)";

      // 2. Map the existing parameters
      command.Parameters.AddWithValue("$name", name);
      command.Parameters.AddWithValue("$url", (object)url ?? DBNull.Value);
      command.Parameters.AddWithValue("$user", (object)user ?? DBNull.Value);
      command.Parameters.AddWithValue("$pass", pass);
      command.Parameters.AddWithValue("$tag", tag ?? "General");

      // 3. Map the NEW parameters
      command.Parameters.AddWithValue("$notes", (object)notes ?? DBNull.Value);
      command.Parameters.AddWithValue("$updatedBy", updatedBy ?? "Windows Desktop");

      // 4. Set the timestamp
      command.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

      command.ExecuteNonQuery();
    }

    public void LearnService(string name, string url, string category)
    {
      if (string.IsNullOrWhiteSpace(name)) return;

      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();

      // 'INSERT OR IGNORE' prevents duplicates if the service already exists
      using var command = connection.CreateCommand();
      command.CommandText = @"
        INSERT OR IGNORE INTO ServiceTemplates (Name, DefaultUrl, Category) 
        VALUES ($name, $url, $category)";

      command.Parameters.AddWithValue("$name", name.Trim());
      command.Parameters.AddWithValue("$url", url?.Trim() ?? (object)DBNull.Value);
      command.Parameters.AddWithValue("$category", category ?? "General");

      command.ExecuteNonQuery();
    }

    public List<ServiceTemplate> GetAllTemplates()
    {
      var templates = new List<ServiceTemplate>();
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();

      var command = connection.CreateCommand();
      command.CommandText = "SELECT Name, DefaultUrl, Category FROM ServiceTemplates ORDER BY Name ASC";

      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        templates.Add(new ServiceTemplate
        {
          Name = reader.GetString(0),
          DefaultUrl = reader.IsDBNull(1) ? null : reader.GetString(1),
          Category = reader.IsDBNull(2) ? null : reader.GetString(2)
        });
      }
      return templates;
    }

    public List<Credential> GetAllCredentials()
    {
      var list = new List<Credential>();
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();

      using var command = connection.CreateCommand();
      command.CommandText = "SELECT Id, ServiceName, ServiceUrl, Username, Password, Tag FROM Credentials WHERE IsDeleted = 0";

      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        list.Add(new Credential
        {
          Id = reader.GetInt32(0).ToString(),
          ServiceName = reader.GetString(1),
          ServiceUrl = reader.IsDBNull(2) ? "" : reader.GetString(2),
          Username = reader.IsDBNull(3) ? "" : reader.GetString(3),
          Password = reader.GetString(4),
          Tag = reader.IsDBNull(5) ? "General" : reader.GetString(5)
        });
      }
      return list;
    }

    private void SeedServiceTemplates()
    {
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();

      // 1. Check if we already have data to avoid double-seeding
      var checkCmd = connection.CreateCommand();
      checkCmd.CommandText = "SELECT COUNT(*) FROM ServiceTemplates";

      // SQLite returns long for COUNT
      long count = (long)(checkCmd.ExecuteScalar() ?? 0L);

      if (count == 0)
      {
        using var transaction = connection.BeginTransaction();
        try
        {
          var insertCmd = connection.CreateCommand();
          insertCmd.Transaction = transaction;

          // 2. Insert the "Famous" services
          // We use 'INSERT OR IGNORE' just in case of a race condition
          insertCmd.CommandText = @"
                INSERT OR IGNORE INTO ServiceTemplates (Name, DefaultUrl, Category) VALUES 
                ('Google', 'https://accounts.google.com', 'Productivity'),
                ('Facebook', 'https://www.facebook.com', 'Social Media'),
                ('GitHub', 'https://github.com/login', 'Development'),
                ('X / Twitter', 'https://x.com/login', 'Social Media'),
                ('Instagram', 'https://www.instagram.com', 'Social Media'),
                ('LinkedIn', 'https://www.linkedin.com/login', 'Professional'),
                ('Netflix', 'https://www.netflix.com/login', 'Entertainment'),
                ('Amazon', 'https://www.amazon.com', 'Shopping'),
                ('Microsoft / Outlook', 'https://login.live.com', 'Productivity'),
                ('Steam', 'https://store.steampowered.com/login', 'Gaming');";

          insertCmd.ExecuteNonQuery();
          transaction.Commit();
        }
        catch (Exception)
        {
          transaction.Rollback();
          // Log error or handle silently
        }
      }
    }
  }
}