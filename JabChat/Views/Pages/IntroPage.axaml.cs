using App.ViewModels;
using Avalonia;
using Avalonia.Controls;
using IntroViewModel = App.ViewModels.Pages.IntroViewModel;

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