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
    Console.WriteLine("Begin");
    try
    {
      Console.WriteLine("SQLitePCL.Batteries_V2.Init()");
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }
    Console.WriteLine("[SQLite] Provider initialized: sqlite3");
    
    App.App.ServiceCollection.AddPlatformServices<BrowserSettingsStorageService, BrowserDatabaseService>();
    
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