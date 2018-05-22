using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;

namespace VSS.TRex.DI
{
  /// <summary>
  /// Provides implementation actions for Direct Injection requirements of a service
  /// </summary>
  public class DIImplementation
  {
    public IServiceProvider ServiceProvider { get; internal set; }
    public IServiceCollection ServiceCollection = new ServiceCollection();

    /// <summary>
    /// Default constructor for DI implementation
    /// </summary>
    public DIImplementation()
    {
    }

    /// <summary>
    /// Constructor accepting a lambda returning a service collection to add to the DI collection
    /// </summary>
    /// <param name="addDI"></param>
    public DIImplementation(Action<IServiceCollection> addDI)
    {
      addDI(ServiceCollection);
    }

    /// <summary>
    /// Adds a set of dependencies according to the supplied lambda
    /// </summary>
    /// <param name="addDI"></param>
    /// <returns></returns>
    public DIImplementation Add(Action<IServiceCollection> addDI)
    {
      addDI(ServiceCollection);
      return this;
    }

    /// <summary>
    /// Adds logging to the DI collection
    /// </summary>
    /// <returns></returns>
    public DIImplementation AddLogging()
    {
      // ### Set up log 4 net related configuration prior to instantiating the logging service
      string loggerRepoName = "VSS";

      //Now set actual logging name
      Log4NetProvider.RepoName = loggerRepoName;

      string logPath;

      if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "log4net.xml")))
      {
        logPath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Setting GetCurrentDirectory path for the config file {logPath}");
      }
      else if (File.Exists(Path.Combine(AppContext.BaseDirectory, "log4net.xml")))
      {
        logPath = Path.Combine(AppContext.BaseDirectory);
        Console.WriteLine($"Setting BaseDirectory path for the config file {logPath}");
      }
      else
      {
        var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
        logPath = Path.GetDirectoryName(pathToExe);
        Console.WriteLine($"Setting alternative path for the config file {logPath}");
      }

      Console.WriteLine("Log path:" + logPath);
      
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4net.xml", loggerRepoName);

      // Create the LoggerFactory instance for the service collection
      ILoggerFactory loggerFactory = new LoggerFactory();
      // Complete configuration of the logger factory
      // LoggerFactory.AddConsole();
      // LoggerFactory.AddDebug();
      loggerFactory.AddProvider(new Log4NetProvider(null));

      // Insert this immediately into the TRex.Logging namesapce to get logging available as early as possible
      Logging.Logger.Inject(loggerFactory);

      // ### Add the logging related services to the collection
      return Add(x => { x.AddSingleton<ILoggerFactory>(loggerFactory); });
    }

    /// <summary>
    /// Builds the service provider, returning it ready for injection
    /// </summary>
    /// <returns></returns>
    public DIImplementation Build()
    {
      ServiceProvider = ServiceCollection.BuildServiceProvider();
      return this;
    }

    /// <summary>
    /// Static method to create a new DIImplementation instance
    /// </summary>
    /// <returns></returns>
    public static DIImplementation New() => new DIImplementation();

    /// <summary>
    /// Performs the Inject operation into the DIContext as a fluent operation from the DIImplementation
    /// </summary>
    public void Inject() => DIContext.Inject(ServiceProvider);

    /// <summary>
    /// A handly shorthand version of .Build().Inject()
    /// </summary>
    public void Complete() => Build().Inject();
  }
}
