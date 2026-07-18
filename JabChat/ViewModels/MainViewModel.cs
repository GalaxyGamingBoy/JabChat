using App.Messages;
using App.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace App.ViewModels;

public partial class MainViewModel(AppSettings settings) : ViewModelBase
{
  public void CheckIntroSeen()
  {
    if (!settings.SeenIntro)
      WeakReferenceMessenger.Default.Send<ShowIntroWindowMessage>();
  }
}