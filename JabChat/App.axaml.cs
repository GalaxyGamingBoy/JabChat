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
    var vm = services.GetRequiredService<MainViewModel>();
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new MainWindow
      {
        DataContext = vm
      };
    }
    else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
    {
      singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView { DataContext = vm };
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
    {
      singleViewPlatform.MainView = new MainView
      {
        DataContext = vm
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}