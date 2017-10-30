using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Hangfire.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using Hangfire.Storage;

namespace SchedulerTests
{
  public class TestControllerBase
  {
    protected IServiceProvider serviceProvider;
    protected IConfigurationStore configStore;
    protected ILoggerFactory loggerFactory;
    protected ILogger log;

    protected void SetupDI()
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

      serviceProvider = serviceCollection.BuildServiceProvider();
      configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      this.loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      log = loggerFactory.CreateLogger<TestControllerBase>();

      Assert.IsNotNull(serviceProvider.GetService<IConfigurationStore>());
      Assert.IsNotNull(serviceProvider.GetService<ILoggerFactory>());
    }


    protected IStorageConnection HangfireConnection()
    {
      // todo doesn't seem to be a way to close connection....?
      MySqlStorage storage = null;
      var hangfireConnectionString = configStore.GetConnectionString("VSPDB");
      log.LogDebug($"ConfigureServices: Scheduler database string: {hangfireConnectionString}.");
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
 