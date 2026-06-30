using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using App.ViewModels;
using App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public partial class App : Application
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    Lang.Resources.Culture = new CultureInfo("en-US");
    
    var collection = new ServiceCollection();
    collection.AddCommonServices();
    
    var services = collection.BuildServiceProvider();
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