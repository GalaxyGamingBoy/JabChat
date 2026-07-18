using System.ComponentModel;
using System.Text.Json;
using App.Settings;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;

namespace App.Services;

public class SettingsService : ISettingsService
{
  public AppSettings Settings { get; set; } = new AppSettings();
  private readonly ISettingsStorageService _storage;

  public SettingsService(ISettingsStorageService storage)
  {
    _storage = storage;
    
    Settings.PropertyChanged += SaveUpdate;
  }


  public async Task Load()
  {
    var data = await _storage.ReadAsync();
    if (data is null) return;
    
    var deserialized = JsonSerializer.Deserialize<AppSettings>(data);
    if (deserialized is null) return;
    
    Settings.PropertyChanged -= SaveUpdate;
    Settings = deserialized;
    Settings.PropertyChanged += SaveUpdate;
  }

  public async Task Save()
  {
    Console.WriteLine("SV");
    var serialized  = JsonSerializer.Serialize(Settings);
    await _storage.WriteAsync(serialized);
  }

  private void SaveUpdate(object? sender, PropertyChangedEventArgs _) => Task.Run(async () => await Save());
}