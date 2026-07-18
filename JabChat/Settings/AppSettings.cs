using App.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.Settings;

public partial class AppSettings : ObservableObject
{
  [ObservableProperty]
  public partial bool Test { get; set; } = false;
}