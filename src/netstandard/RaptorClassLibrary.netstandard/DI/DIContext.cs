using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.TRex.Interfaces;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.DI
{
  /// <summary>
  /// Forms the reference context for elements provided through dependency injection
  /// </summary>
  public static class DIContext
  {
    private static IServiceProvider ServiceProvider { get; set; }

    public static IRenderingFactory RenderingFactory { get; internal set; }

    public static ILoggerFactory LoggerFactory { get; internal set; }

    public static IStorageProxyFactory StorageProxyFactory { get; internal set; }

    public static void Inject(DIImplementation implementation) => Inject(implementation.ServiceProvider);

    public static void Inject(IServiceProvider serviceProvider)
    {
      ServiceProvider = serviceProvider;

      RenderingFactory = serviceProvider.GetService<IRenderingFactory>();

      LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      // Inject the logger factory into the logging namespace for use
      Logging.Logger.Inject(LoggerFactory);

      StorageProxyFactory = serviceProvider.GetService<IStorageProxyFactory>();
      StorageProxy.Inject(StorageProxyFactory);
    }
  }
}
