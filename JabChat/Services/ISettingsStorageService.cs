namespace App.Services;

public interface ISettingsStorageService
{
  Task<string?> ReadAsync();
  Task WriteAsync(string data);
}