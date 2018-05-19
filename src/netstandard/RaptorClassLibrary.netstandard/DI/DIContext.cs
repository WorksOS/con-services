using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.DI
{
  public static class DIContext
  {
    private static IServiceProvider ServiceProvider { get; set; }

    public static IRenderingFactory RenderingFactory { get; internal set; }

    public static ILoggerFactory LoggerFactory { get; internal set; }

    public static void Inject(DIImplementation implementation) => Inject(implementation.ServiceProvider);

    public static void Inject(IServiceProvider serviceProvider)
    {
      ServiceProvider = serviceProvider;

      RenderingFactory = serviceProvider.GetService<IRenderingFactory>();

      LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      // Complete configuration of the logger factory
//      LoggerFactory.AddConsole();
//      LoggerFactory.AddDebug();
      LoggerFactory.AddProvider(new Log4NetProvider(null));

      // Inject the logger factory into the logging namespace for use
      Logging.Logger.Inject(LoggerFactory);
    }
  }
}
