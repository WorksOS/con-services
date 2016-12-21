using System;

namespace VSS.UnifiedProductivity.Service.Utils
{
  public class DependencyInjectionProvider
  {
    public DependencyInjectionProvider(IServiceProvider provider)
    {
      if (provider != null)
      {
        // for unit tests, need to test e.g. certain components missing
        //       also only 1 instantiation can be done per assembly
        //if (ServiceProvider == null)
          ServiceProvider = provider;
        //else
        //  throw new ArgumentException("ServiceProvider is defined already");
      }
      else
        throw new ArgumentException("No serviceProvider has been made available");
    }

    public static void CleanDependencyInjection()
    {
      ServiceProvider = null;
    }

      public static IServiceProvider ServiceProvider { get; private set; } = null;
  }
}



