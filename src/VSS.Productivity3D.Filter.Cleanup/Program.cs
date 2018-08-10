using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;

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
    public const string LoggerRepoName = "FilterCleanup";


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

      Log4NetProvider.RepoName = LoggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(LoggerRepoName, "log4net.xml");
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(LoggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);

      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
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
