using System.Collections.ObjectModel;
using App.Messages;
using App.Services;
using App.Settings;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace App.ViewModels;

public partial class IntroViewModel : ViewModelBase, IDisposable
{
  [ObservableProperty]
  public partial int SelectedCarouselIndex { get; set; } = 0;
  
  public ObservableCollection<string> Carousels { get; } = [
      "Send messages to any XMPP user",
      "Share status updates",
      "Familiar design across all your devices"
  ];
  
  public IEnumerable<int> CarouselIndexes => Enumerable.Range(0, Carousels.Count);

  private readonly DispatcherTimer _timer = new DispatcherTimer();
  
  private readonly AppSettings _settings;
  
  public IntroViewModel(AppSettings settings)
  {
    _settings = settings;
    
    _timer.Interval = TimeSpan.FromSeconds(6);
    _timer.Tick += Timer_Tick;
    
    Carousels.CollectionChanged += (_, __) => OnPropertyChanged(nameof(CarouselIndexes));
  }

  [RelayCommand]
  private void Later()
  {
    _settings.SeenIntro = true;
    WeakReferenceMessenger.Default.Send<NavigatorPopAllMessage>();
  } 

  public void StartCarousel() => _timer.Start();
  
  public void Dispose()
  {
    _timer.Stop();
    _timer.Tick -= Timer_Tick;
  }

  private void Timer_Tick(object? sender, EventArgs _)
  {
    SelectedCarouselIndex++;
  }
}