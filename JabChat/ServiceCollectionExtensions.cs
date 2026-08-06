using App.Services;
using App.Settings;
using App.ViewModels;
using App.ViewModels.Pages.Accounts;
using App.Views;
using App.Views.Pages.Accounts;
using FluentMigrator.Runner;
using JabChat.Migrations;
using Microsoft.Extensions.DependencyInjection;
using IntroPage = App.Views.Pages.IntroPage;
using IntroViewModel = App.ViewModels.Pages.IntroViewModel;

namespace App;

public static class ServiceCollectionExtensions
{
  public static void AddCommonServices(this IServiceCollection collection)
  {
    // View Models
    collection.AddTransient<MainViewModel>();
    collection.AddTransient<IntroViewModel>();
    collection.AddTransient<AddAccountViewModel>();
      
    // Main UI
    collection.AddTransient<MainWindow>();
    collection.AddTransient<MainView>();
    
    // Pages
    collection.AddTransient<IntroPage>();
    collection.AddTransient<AddAccountPage>();
    
    // Services
    collection.AddSingleton<ISettingsService, SettingsService>();
    collection.AddTransient<IDatabaseConnection, DatabaseConnection>();
    
    // Settings
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