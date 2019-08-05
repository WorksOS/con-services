using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using Serilog;
using VSS.Serilog.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VSS.Productivity3D.Filter.Cleanup
{
  internal class Program
  {
    protected static IServiceProvider ServiceProvider;
    protected static IConfigurationStore ConfigStore;
    protected static IServiceExceptionHandler ServiceExceptionHandler;
    protected static ILoggerFactory Logger;
    protected static IFilterRepository FilterRepository;
    protected static ILogger Log;

    private static void Main()
    {
      Console.WriteLine("***** Filter clean up task starting ***** ");
      Thread.Sleep(10000);
      SetupDi();
      var cleanupTask = new FilterCleanupTask(ConfigStore, Logger, ServiceExceptionHandler, FilterRepository);

      cleanupTask.FilterCleanup().ConfigureAwait(false);
      Log.LogDebug("***** Finished Processing filter cleanup ***** ");
    }

    private static void SetupDi()
    {
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging()
                       .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure()))
                       .AddSingleton<IConfigurationStore, GenericConfiguration>();

      serviceCollection.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      serviceCollection.AddTransient<IFilterRepository, FilterRepository>();
      serviceCollection.AddTransient<IErrorCodesProvider, FilterErrorCodesProvider>();

      ServiceProvider = serviceCollection.BuildServiceProvider();

      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      Log = Logger.CreateLogger<Program>();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      FilterRepository = ServiceProvider.GetRequiredService<IFilterRepository>();
    }
  }
}
