using App.Messages;
using App.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace App.Views;

public partial class MainWindow : Window
{
  public MainWindow(MainViewModel vm, IServiceProvider provider)
  {
    InitializeComponent();
    DataContext = vm;
    
    WeakReferenceMessenger.Default.Register<MainWindow, FetchServiceProviderMessage>(this, (recipient, message) =>
      message.Reply(provider));
  }
}