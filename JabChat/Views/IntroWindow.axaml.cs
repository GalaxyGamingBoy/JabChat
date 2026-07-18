using App.ViewModels;
using Avalonia.Controls;
using Avalonia.Threading;

namespace App.Views;

public partial class IntroWindow : Window
{
  public IntroWindow(IntroViewModel vm)
  {
    InitializeComponent();
    DataContext = vm;
    
    vm.StartCarousel();
  }
}