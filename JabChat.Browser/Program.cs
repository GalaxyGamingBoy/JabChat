using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using App;
using App.Browser.Services;

internal sealed partial class Program
{
  private static Task Main(string[] args)
  {
    App.App.ServiceCollection.AddPlatformServices<BrowserSettingsStorageService>();
    
    return BuildAvaloniaApp()
      .WithInterFont()
#if DEBUG
      .WithDeveloperTools()
#endif
      .StartBrowserAppAsync("out");
  }

  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App.App>();
}