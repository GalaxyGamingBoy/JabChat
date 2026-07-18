using App.Services;
using App.Settings;
using App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class ServiceCollectionExtensions
{
  public static void AddCommonServices(this IServiceCollection collection)
  {
    collection.AddTransient<MainViewModel>();
    
    collection.AddSingleton<ISettingsService, SettingsService>();
    collection.AddSingleton<AppSettings>(s => s.GetRequiredService<ISettingsService>().Settings);
  }

  public static void AddPlatformServices<T>(this IServiceCollection collection)
    where T : class, ISettingsStorageService
  {
    collection.AddSingleton<ISettingsStorageService, T>();
  }
}