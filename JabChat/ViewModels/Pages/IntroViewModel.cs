using System.Collections.ObjectModel;
using App.Messages;
using App.Settings;
using App.ViewModels.Pages.Accounts;
using App.Views.Pages.Accounts;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace App.ViewModels.Pages;

public partial class IntroViewModel : ViewModelBase, IDisposable
{
  [ObservableProperty]
  public partial int SelectedCarouselIndex { get; set; } = 0;
  
  public ObservableCollection<string> Carousels { get; } = [
      Lang.Resources.IntroPage_Carousel_Messenger,
      Lang.Resources.IntroPage_Carousel_Status,
      Lang.Resources.IntroPage_Carousel_CrossPlatform
  ];
  
  public IEnumerable<int> CarouselIndexes => Enumerable.Range(0, Carousels.Count);

  private readonly DispatcherTimer _timer = new DispatcherTimer();

  private readonly AppSettings _settings;

  private readonly IServiceProvider _provider;
  
  public IntroViewModel(AppSettings settings, IServiceProvider provider)
  {
    _settings = settings;
    _provider = provider;
    
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

  [RelayCommand]
  private void Add()
  {
    // _settings.SeenIntro = true;

    var page = _provider.GetRequiredService<AddAccountPage>();
    WeakReferenceMessenger.Default.Send(new NavigatorPushMessage(page));
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