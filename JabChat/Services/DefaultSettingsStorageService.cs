namespace App.Services;

public sealed class DefaultSettingsStorageService : ISettingsStorageService
{
  private string SettingsDirectory
    => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JabChat"); 
  private string SettingsFile
    => Path.Combine(SettingsDirectory, "settings.json");
  
  public Task<string?> ReadAsync()
  {
    if (!File.Exists(SettingsFile))
      return Task.FromResult<string?>(null);
    
    return Task.FromResult<string?>(File.ReadAllText(SettingsFile));
  }

  public async Task WriteAsync(string data)
  {
    if (!Directory.Exists(SettingsDirectory))
      Directory.CreateDirectory(SettingsDirectory);
    await File.WriteAllTextAsync(SettingsFile, data);
  }
}