using App.ViewModels;
using Avalonia;
using Avalonia.Controls;

namespace App.Views.Pages;

public partial class IntroPage : ContentPage
{
  public IntroPage(IntroViewModel vm)
  {
    InitializeComponent();
    DataContext = vm;
    
    vm.StartCarousel();
  }

  protected override void OnSizeChanged(SizeChangedEventArgs e)
  {
    base.OnSizeChanged(e);
    Console.WriteLine($"OnSizeChanged: {e.NewSize}");
  }
}