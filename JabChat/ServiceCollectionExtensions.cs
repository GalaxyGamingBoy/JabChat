using App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class ServiceCollectionExtensions
{
  public static void AddCommonServices(this IServiceCollection collection)
  {
    collection.AddTransient<MainViewModel>();
  }
}