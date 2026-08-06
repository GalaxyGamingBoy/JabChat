using App.Messages;
using App.Settings;
using App.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace App.ViewModels;

public partial class MainViewModel(AppSettings settings, IServiceProvider provider) : ViewModelBase
{
  public void CheckIntroSeen()
  {
    if (!settings.SeenIntro)
      WeakReferenceMessenger.Default.Send(new NavigatorPushMessage(provider.GetRequiredService<IntroPage>()));
  }
}