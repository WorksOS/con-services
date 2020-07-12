using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    /// Determines the default behaviour of Obtain<T>: Optional or mandatory presence in the service provider
    /// </summary>
    public static bool DefaultIsRequired = false;

    /// <summary>
    /// Injects the service provider collection in the TRex DI context.
    /// </summary>
    public static void Inject(IServiceProvider serviceProvider)
    {
      // Register the service provider with the overall TRex DI context
      ServiceProvider = serviceProvider;

      // Advise the TRex Logger namespace of the logger factory to use in case
      // DIBuilder.AddLogging was not used in the DI construction phase.
      Logging.Logger.Inject(serviceProvider.GetService<ILoggerFactory>());
    }

    /// <summary>
    /// Removes the service provider collection from the TRex DI context.
    /// </summary>
    public static void Close()
    {
      Logging.Logger.Inject(null);
      ServiceProvider = null;
    }

    /// <summary>
    /// Obtain a service instance matching a provided type T. If there is no ServiceProvider or the service provider
    /// returns a default service then return that default/null service
    /// </summary>
    public static T ObtainOptional<T>() => ServiceProvider == null ? default(T) : ServiceProvider.GetService<T>();

    /// <summary>
    /// Obtain a service instance matching a provided type T
    ///  </summary>
    /// <returns>The service implementing T. If there is no ServiceProvider or the service provider returns a default
    /// service then throw an exception (ie: All Obtains are required services</returns>
    public static T ObtainRequired<T>()
    {
      if (ServiceProvider == null)
        throw new Exception("DIContext service provider not available");

      var result = ServiceProvider.GetRequiredService<T>();

      if (result == null)
        throw new Exception("Required service not available, null result on GetRequiredService");

      return result;
    }

    /// <summary>
    /// Obtain a service instance matching a provided type T using the default required/optional mechanism
    ///  </summary>
    /// <returns>The service implementing T. If there is no ServiceProvider or the service provider returns a default
    /// service then throw an exception (ie: All Obtains are required services</returns>
    public static T Obtain<T>() => DefaultIsRequired ? ObtainRequired<T>() : ObtainOptional<T>();

    /// <summary>
    /// Obtain all service instances matching a provided type T
    ///  </summary>
    /// <returns>The services implementing T. If there is no ServiceProvider available then return null</returns>
    public static IEnumerable<T> ObtainMany<T>() => ServiceProvider?.GetServices<T>();
  }
}
