namespace App.Services;

public interface IDatabaseService
{
  string GetDatabaseUri();
  Task SaveAsync();
}