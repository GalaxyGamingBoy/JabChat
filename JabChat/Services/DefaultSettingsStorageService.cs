namespace App.Services;

public sealed class DefaultSettingsStorageService : ISettingsStorageService
{
  private string SettingsDirectory
    => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JabChat"); 
  private string SettingsFile
    => Path.Combine(SettingsDirectory, "settings.json");
  
  public async Task<string?> ReadAsync()
  {
    if (!File.Exists(SettingsFile))
      return null;
    
    return await File.ReadAllTextAsync(SettingsFile);
  }

  public async Task WriteAsync(string data)
  {
    if (!Directory.Exists(SettingsDirectory))
      Directory.CreateDirectory(SettingsDirectory);
    await File.WriteAllTextAsync(SettingsFile, data);
  }
}