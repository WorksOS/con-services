using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;

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


    private static void Main(string[] args)
    {
      SetupDi(); 
      var cleanupTask = new FilterCleanupTask(ConfigStore, Logger, ServiceExceptionHandler, FilterRepository);
      
      cleanupTask.FilterCleanup().ConfigureAwait(false);
      Log.LogDebug("***** Finished Processing filter cleanup ***** ");
    }

    private static void SetupDi()
    {
      var serviceCollection = new ServiceCollection();

      const string loggerRepoName = "FilterCleanup";
      Log4NetProvider.RepoName = loggerRepoName;
      var logPath = Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();
      serviceCollection.AddTransient<IFilterRepository, FilterRepository>();
      ServiceProvider = serviceCollection.BuildServiceProvider();

      Logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      Log = Logger.CreateLogger<Program>();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      ServiceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      FilterRepository = ServiceProvider.GetRequiredService<IFilterRepository>();
    }
  }
}
