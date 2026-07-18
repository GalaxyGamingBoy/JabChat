using App.Messages;
using App.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace App.Views;

public partial class MainView : UserControl
{
  public MainView()
  {
    InitializeComponent();
    
    RegisterIntroWindowMessage();
  }

  public MainView(IServiceProvider provider)
  {
    InitializeComponent();
    
    WeakReferenceMessenger.Default.Register<MainView, FetchServiceProviderMessage>(this, (recipient, message) =>
      message.Reply(provider));
    
    RegisterIntroWindowMessage();
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);
    
    ((MainViewModel)DataContext!).CheckIntroSeen();
  }
  
  private void RegisterIntroWindowMessage() => 
    WeakReferenceMessenger.Default.Register<MainView, ShowIntroWindowMessage>(this, async (recipient, message) => 
    { 
      var prov = WeakReferenceMessenger.Default.Send<FetchServiceProviderMessage>();
      var dialog = prov.Response.GetRequiredService<Pages.IntroPage>();
      await Navigator.PushAsync(dialog);
    });
}