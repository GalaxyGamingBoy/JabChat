using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

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
  
  public IntroViewModel()
  {
    _timer.Interval = TimeSpan.FromSeconds(6);
    _timer.Tick += Timer_Tick;
    
    Carousels.CollectionChanged += (_, __) => OnPropertyChanged(nameof(CarouselIndexes));
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