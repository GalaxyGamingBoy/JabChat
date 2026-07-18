using Microsoft.Data.Sqlite;

namespace App.Services;

public interface IDatabaseConnection
{
  Task<SqliteConnection> GetConnection();
  string GetConnectionString();
}