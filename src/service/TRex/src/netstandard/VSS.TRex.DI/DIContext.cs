using System;
using Microsoft.Extensions.DependencyInjection;

namespace VSS.TRex.DI
{
  /// <summary>
  /// Forms the reference context for elements provided through dependency injection
  /// </summary>
  public static class DIContext
  {
    private static IServiceProvider ServiceProvider { get; set; }

    public static void Inject(DIBuilder implementation) => Inject(implementation.ServiceProvider);

    /// <summary>
    /// Injects the service provider collection in the TRex DI context.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static void Inject(IServiceProvider serviceProvider)
    {
      ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Ejects the service provider collection from the TRex DI context.
    /// </summary>
    public static void Eject()
    {
      ServiceProvider = null;
    }

    /// <summary>
    /// Obtain a service instance matching a provided type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Obtain<T>() => ServiceProvider.GetService<T>();
  }
}
