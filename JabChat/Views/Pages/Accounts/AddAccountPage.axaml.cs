using App.ViewModels.Pages.Accounts;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace App.Views.Pages.Accounts;

public partial class AddAccountPage : ContentPage
{
  public AddAccountPage(AddAccountViewModel vm)
  {
    InitializeComponent();
    DataContext = vm;
  }
}