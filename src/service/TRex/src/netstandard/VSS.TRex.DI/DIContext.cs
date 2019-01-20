using System;
using System.Collections.Generic;
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
    /// Removes the service provider collection from the TRex DI context.
    /// </summary>
    public static void Close()
    {
      ServiceProvider = null;
    }

    /// <summary>
    /// Obtain a service instance matching a provided type T
    ///  </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The service implementing T. If there is no ServiceProvider available then return a default(T)</returns>
    public static T Obtain<T>() => ServiceProvider == null ? default(T) : ServiceProvider.GetService<T>();

    /// <summary>
    /// Obtain all service instances matching a provided type T
    ///  </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>The services implementing T. If there is no ServiceProvider available then return null</returns>
    public static IEnumerable<T> ObtainMany<T>() => ServiceProvider?.GetServices<T>();
  }
}
