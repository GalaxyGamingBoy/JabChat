using App.ViewModels;
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
}