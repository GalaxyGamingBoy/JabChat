using Microsoft.Data.Sqlite;

namespace App.Services;

public class DatabaseConnection(IDatabaseService database) : IDatabaseConnection
{
  public async Task<SqliteConnection> GetConnection()
  {
    var connection = new SqliteConnection(GetConnectionString());
    await connection.OpenAsync();
    
    return connection;
  }

  public string GetConnectionString()
  {
    var uri = database.GetDatabaseUri();
    return $"Data Source={uri}";
  }
}