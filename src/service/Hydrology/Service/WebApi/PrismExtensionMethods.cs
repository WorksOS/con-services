#if NET_4_7
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.ServiceLocation;

namespace VSS.Hydrology.WebApi
{
  public static class PrismExtensionMethods
  {
    public static IServiceCollection AddPrismServiceResolution(this IServiceCollection service)
    {
      new BootStrapper(service.BuildServiceProvider()).Init();
      return service;
    }

    public static IServiceCollection AddPrismService<T>(this IServiceCollection service) where T : class
    {
      return service.AddTransient(provider => ServiceLocator.Current.GetInstance<T>());
    }
  }
}
#endif
