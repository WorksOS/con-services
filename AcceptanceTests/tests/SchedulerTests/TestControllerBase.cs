using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Hangfire.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using Hangfire.Storage;

namespace SchedulerTests
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    protected IConfigurationStore ConfigStore;
    protected ILoggerFactory LoggerFactory;
    protected ILogger Log;

    protected void SetupDi()
    {
      const string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection
        .AddSingleton(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      this.LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
      Log = loggerFactory.CreateLogger<TestControllerBase>();

      Assert.IsNotNull(ServiceProvider.GetService<IConfigurationStore>());
      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }


    protected IStorageConnection HangfireConnection()
    {
      // todo doesn't seem to be a way to close connection....?
      MySqlStorage storage = null;
      var hangfireConnectionString = ConfigStore.GetConnectionString("VSPDB");
      Log.LogDebug($"ConfigureServices: Scheduler database string: {hangfireConnectionString}.");
      storage = new MySqlStorage(hangfireConnectionString);
      IStorageConnection hangoutConnection = storage.GetConnection();
      return hangoutConnection;
    }

    protected RecurringJobDto GetJob(IStorageConnection hangoutConnection, string jobName)
    {
      RecurringJobDto theJob = null;
      List<RecurringJobDto> recurringJobs = hangoutConnection.GetRecurringJobs();
      recurringJobs.ForEach(delegate (RecurringJobDto job)
      {
        if (job.Id == jobName)
        {
          theJob = job;
        }
      });
      return theJob;
    }
  }
}
 