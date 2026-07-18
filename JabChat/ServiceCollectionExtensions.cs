using App.Services;
using App.Settings;
using App.ViewModels;
using App.Views;
using FluentMigrator.Runner;
using JabChat.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class ServiceCollectionExtensions
{
  public static void AddCommonServices(this IServiceCollection collection)
  {
    collection.AddTransient<MainViewModel>();
    collection.AddTransient<IntroViewModel>();
      
    collection.AddTransient<MainWindow>();
    collection.AddTransient<IntroWindow>();
    
    collection.AddSingleton<ISettingsService, SettingsService>();
    collection.AddTransient<IDatabaseConnection, DatabaseConnection>();
    
    collection.AddSingleton<AppSettings>(s => s.GetRequiredService<ISettingsService>().Settings);

    collection
      .AddFluentMigratorCore()
      .ConfigureRunner(runner =>
        runner
          .AddSQLite()
          .WithGlobalConnectionString(sp => sp.GetRequiredService<IDatabaseConnection>().GetConnectionString())
          .ScanIn(typeof(Runner).Assembly).For.Migrations()
      );
  }

  public static void AddPlatformServices<T, D>(this IServiceCollection collection)
    where T : class, ISettingsStorageService
    where D : class, IDatabaseService
  {
    collection.AddSingleton<ISettingsStorageService, T>();
    collection.AddSingleton<IDatabaseService, D>();
  }
}