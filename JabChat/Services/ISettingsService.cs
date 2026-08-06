using App.Settings;

namespace App.Services;

public interface ISettingsService
{
  AppSettings Settings { get; set; }

  public Task Load();
  public Task Save();
}