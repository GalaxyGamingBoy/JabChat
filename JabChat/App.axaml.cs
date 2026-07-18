using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using App.Services;
using App.Settings;
using Avalonia.Markup.Xaml;
using App.ViewModels;
using App.Views;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public partial class App : Application
{
  public static readonly ServiceCollection ServiceCollection = [];
  
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public override async void OnFrameworkInitializationCompleted()
  {
    Lang.Resources.Culture = new CultureInfo("en-US");

    ServiceCollection.AddCommonServices();
    
    var services = ServiceCollection.BuildServiceProvider();
 
    // Load Settings
    var settingsService = services.GetRequiredService<ISettingsService>();
    await settingsService.Load();

    // Load Migrations
    var migrationRunner = services.GetRequiredService<IMigrationRunner>();
    migrationRunner.MigrateUp();
    
    // Load UI
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = services.GetRequiredService<MainWindow>();
    }
    else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
    {
      singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView(services)
      {
        DataContext = services.GetRequiredService<MainViewModel>()
      };
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
    {
      singleViewPlatform.MainView = new MainView(services)
      {
        DataContext = services.GetRequiredService<MainViewModel>()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}