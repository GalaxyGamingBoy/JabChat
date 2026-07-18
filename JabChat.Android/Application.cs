using Android.App;
using Android.Runtime;
using App.Services;
using Avalonia;
using Avalonia.Android;

namespace App.Android
{
  [Application]
  public class Application : AvaloniaAndroidApplication<App>
  {
    protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
      App.ServiceCollection.AddPlatformServices<DefaultSettingsStorageService, DefaultDatabaseService>();
      
      return base.CustomizeAppBuilder(builder)
        .WithInterFont();
    }
  }
}