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

      using var transaction = connection.BeginTransaction();
      try
      {
        var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Credentials (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                
                -- Versioning Columns --
                GroupId INTEGER NOT NULL,          -- Anchor for the account family
                Version INTEGER DEFAULT 1,         -- Increments per update
                IsArchived INTEGER DEFAULT 0,      -- 0 = Active, 1 = History
                
                -- Core Data --
                ServiceName TEXT NOT NULL,
                ServiceUrl TEXT,
                Username TEXT,
                Password TEXT NOT NULL,
                Tag TEXT,
                Notes TEXT,
                
                -- Audit Metadata --
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                UpdatedBy TEXT,                    -- Machine name/User ID
                
                -- Recycle Bin Logic --
                IsDeleted INTEGER DEFAULT 0,       -- 1 = In Trash
                DeletedAt DATETIME NULL            -- Timestamp for auto-purge logic
            );

            CREATE TABLE IF NOT EXISTS ServiceTemplates (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT UNIQUE,
                DefaultUrl TEXT,
                Category TEXT, 
                IconPath TEXT
            );";

        command.ExecuteNonQuery();

        // Indexing for performance
        command.CommandText = "CREATE INDEX IF NOT EXISTS idx_groupid ON Credentials (GroupId);";
        command.ExecuteNonQuery();

        // Indexing for the Dashboard (Filtering Live/Non-deleted items)
        command.CommandText = "CREATE INDEX IF NOT EXISTS idx_live_credentials ON Credentials (IsArchived, IsDeleted);";
        command.ExecuteNonQuery();

        transaction.Commit();
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

      // Using a transaction to ensure the 'Anchor' (GroupId) is set correctly
      using var transaction = connection.BeginTransaction();

      try
      {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        // 1. Insert the record with GroupId = 0 (placeholder)
        // Note: Version defaults to 1 and IsArchived to 0 via DB schema
        command.CommandText = @"
            INSERT INTO Credentials 
            (GroupId, ServiceName, ServiceUrl, Username, Password, Tag, Notes, UpdatedBy) 
            VALUES 
            (0, $name, $url, $user, $pass, $tag, $notes, $updatedBy);
            
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$url", (object)url ?? DBNull.Value);
        command.Parameters.AddWithValue("$user", (object)user ?? DBNull.Value);
        command.Parameters.AddWithValue("$pass", pass);
        command.Parameters.AddWithValue("$tag", tag ?? "General");
        command.Parameters.AddWithValue("$notes", (object)notes ?? DBNull.Value);
        command.Parameters.AddWithValue("$updatedBy", updatedBy ?? Environment.MachineName);

        // 2. Execute and capture the newly generated Id
        long newId = (long)command.ExecuteScalar();

        // 3. 'Anchor' the family: Set GroupId equal to the row's own Id
        command.CommandText = "UPDATE Credentials SET GroupId = Id WHERE Id = $id";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("$id", newId);
        command.ExecuteNonQuery();

        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
    }

    public void UpdateCredential(int id, string name, string url, string user, string pass, string tag, string notes, string updatedBy)
    {
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();
      using var transaction = connection.BeginTransaction();

      try
      {
        // 1. Get current Version and GroupId of the record we are editing
        using var getCmd = connection.CreateCommand();
        getCmd.CommandText = "SELECT GroupId, Version FROM Credentials WHERE Id = $id";
        getCmd.Parameters.AddWithValue("$id", id);
        getCmd.Transaction = transaction;

        int groupId = 0;
        int currentVersion = 0;
        using (var reader = getCmd.ExecuteReader())
        {
          if (reader.Read())
          {
            groupId = reader.GetInt32(0);
            currentVersion = reader.GetInt32(1);
          }
        }

        // 2. Archive the old record
        using var archiveCmd = connection.CreateCommand();
        archiveCmd.Transaction = transaction;
        archiveCmd.CommandText = "UPDATE Credentials SET IsArchived = 1 WHERE Id = $id";
        archiveCmd.Parameters.AddWithValue("$id", id);
        archiveCmd.ExecuteNonQuery();

        // 3. Insert the new version
        using var insertCmd = connection.CreateCommand();
        insertCmd.Transaction = transaction;
        insertCmd.CommandText = @"
            INSERT INTO Credentials 
            (GroupId, Version, ServiceName, ServiceUrl, Username, Password, Tag, Notes, UpdatedBy) 
            VALUES 
            ($groupId, $newVersion, $name, $url, $user, $pass, $tag, $notes, $updatedBy);";

        insertCmd.Parameters.AddWithValue("$groupId", groupId);
        insertCmd.Parameters.AddWithValue("$newVersion", currentVersion + 1);
        insertCmd.Parameters.AddWithValue("$name", name);
        insertCmd.Parameters.AddWithValue("$url", (object)url ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("$user", (object)user ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("$pass", pass);
        insertCmd.Parameters.AddWithValue("$tag", tag ?? "General");
        insertCmd.Parameters.AddWithValue("$notes", (object)notes ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("$updatedBy", updatedBy ?? Environment.MachineName);

        insertCmd.ExecuteNonQuery();
        transaction.Commit();
      }
      catch (Exception)
      {
        transaction.Rollback();
        throw;
      }
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

    // <summary>
    /// Retrieves all active credentials with full audit details.
    /// </summary>
    public List<Credential> GetAllCredentials()
    {
      var list = new List<Credential>();
      using var connection = new SqliteConnection(ConnectionString);
      connection.Open();

      using var command = connection.CreateCommand();
      command.CommandText = @"
        SELECT 
            Id, GroupId, Version, ServiceName, ServiceUrl, 
            Username, Password, Tag, Notes, 
            CreatedAt, UpdatedAt, UpdatedBy, DeletedAt 
        FROM Credentials 
        WHERE IsDeleted = 0 AND IsArchived = 0 
        ORDER BY ServiceName ASC";

      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        list.Add(new Credential
        {
          Id = reader.GetInt32(0),
          GroupId = reader.GetInt32(1),
          Version = reader.GetInt32(2),
          ServiceName = reader.GetString(3),
          ServiceUrl = reader.IsDBNull(4) ? "" : reader.GetString(4),
          Username = reader.IsDBNull(5) ? "" : reader.GetString(5),
          Password = reader.GetString(6),
          Tag = reader.IsDBNull(7) ? "General" : reader.GetString(7),

          // Audit & Extra Details
          Notes = reader.IsDBNull(8) ? "" : reader.GetString(8),
          CreatedAt = reader.IsDBNull(9) ? "" : reader.GetString(9),
          UpdatedAt = reader.IsDBNull(10) ? "" : reader.GetString(10),
          UpdatedBy = reader.IsDBNull(11) ? "Unknown Device" : reader.GetString(11),

          // NEW: Mapping the deletion timestamp
          DeletedAt = reader.IsDBNull(12) ? null : reader.GetString(12)
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