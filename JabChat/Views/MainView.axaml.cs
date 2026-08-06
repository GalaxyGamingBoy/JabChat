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
    RegisterNavigator();
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);
    
    ((MainViewModel)DataContext!).CheckIntroSeen();
  }
  
  private void RegisterNavigator()
  {
    WeakReferenceMessenger.Default.Register<MainView, NavigatorPushMessage>(this, async (recipient, message) =>
      await Navigator.PushAsync(message.Page));
    WeakReferenceMessenger.Default.Register<MainView, NavigatorPopMessage>(this, async (recipient, message) =>
      await Navigator.PopAsync());
    WeakReferenceMessenger.Default.Register<MainView, NavigatorPopAllMessage>(this, async (recipient, message) =>
      await Navigator.PopToRootAsync());
  }
}