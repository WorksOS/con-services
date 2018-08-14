using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using Hangfire.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using Hangfire.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Models;

namespace SchedulerTestsFilterCleanup
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    protected IConfigurationStore ConfigStore;
    protected ILoggerFactory LoggerFactory;
    protected ILogger Log;
    protected IRaptorProxy RaptorProxy;
    private readonly string loggerRepoName = "UnitTestLogTest";


    protected void SetupDi()
    {
      var serviceCollection = new ServiceCollection();

      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName, "log4nettest.xml");
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IOptions<MemoryCacheOptions>>(new MemoryCacheOptions())
        .AddTransient<IMemoryCache, MemoryCache>()
        .AddTransient<IRaptorProxy, RaptorProxy>()
        .AddTransient<ISchedulerProxy, SchedulerProxy>();
      
      ServiceProvider = serviceCollection.BuildServiceProvider();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      this.LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
      Log = loggerFactory.CreateLogger<TestControllerBase>();
      
      RaptorProxy = new RaptorProxy(ConfigStore, LoggerFactory);

      Assert.IsNotNull(ServiceProvider.GetService<IConfigurationStore>());
      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
      Assert.IsNotNull(ServiceProvider.GetService<IRaptorProxy>());
      Assert.IsNotNull(ServiceProvider.GetService<ISchedulerProxy>());
    }


    protected IStorageConnection HangfireConnection()
    {
      // doesn't seem to be a way to close the hangouts connection.
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
      recurringJobs.ForEach(delegate(RecurringJobDto job)
      {
        if (job.Id == jobName)
        {
          theJob = job;
        }
      });
      return theJob;
    }


    protected int WriteToProjectDBImportedFile(string projectDbConnectionString, ImportedFileProject importedFile)
    {
      var dbConnection = new MySqlConnection(projectDbConnectionString);
      dbConnection.Open();

      var insertCommand = string.Format(
        "INSERT ImportedFile " +
        "    (fk_ProjectUID, ImportedFileUID, LegacyImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID,  IsDeleted, LastActionedUTC)" +
        "  VALUES " +
        "    (@ProjectUid, @ImportedFileUid, @LegacyImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc, @DxfUnitsType, 0, @LastActionedUtc)");

      int insertedCount = dbConnection.Execute(insertCommand, importedFile);
      dbConnection.Close();
      return insertedCount;
    }

    protected int WriteToProjectDBCustomerProjectAndProject(string projectDbConnectionString, ImportedFileProject importedFile)
    {
      // importedFile table depends on a project (for legacyProjectID) and CustomerProject (for legacyCustomerId) rows existing
      var dbConnection = new MySqlConnection(projectDbConnectionString);
      dbConnection.Open();

      var insertCommand = string.Format(
        "INSERT CustomerProject " +
        "    (fk_CustomerUID, fk_ProjectUID, LegacyCustomerID, LastActionedUTC)" +
        "  VALUES " +
        "    (@CustomerUid, @ProjectUid, @LegacyCustomerId, @LastActionedUtc)");

      var insertedCount = dbConnection.Execute(insertCommand, importedFile);

      if (insertedCount == 1)
      {
        // for some reason the LegacyProjectId doesn't get set to the actual value when syntax as above.
        // In that case it gets set to the next generatedID
        insertCommand = string.Format(
          $"INSERT Project (ProjectUID, LegacyProjectID, Name, fk_ProjectTypeID, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate) VALUES (@ProjectUid, {importedFile.LegacyProjectId}, \"the project name\", 0, \"\", \"\", 20170101, 20180101)");

        insertedCount += dbConnection.Execute(insertCommand, importedFile);
      }

      dbConnection.Close();
      return insertedCount;
    }
    protected int WriteNhOpDbImportedFileAndHistory(string projectDbConnectionString, ImportedFileNhOp importedFile)
    {
      var dbConnection = new SqlConnection(projectDbConnectionString);
      dbConnection.Open();

      int insertedCount = 0;

      var insertCommand = string.Format(
        " SET IDENTITY_INSERT ImportedFile ON;"  + 
        " INSERT ImportedFile " +
        "    (ID, fk_CustomerID, fk_ProjectID, Name, fk_ImportedFileTypeID, SurveyedUTC, fk_DXFUnitsTypeID) " +
        "  VALUES " +
        "    (@LegacyImportedFileId, @LegacyCustomerId, @LegacyProjectId, @Name, @ImportedFileType, @SurveyedUtc, @DxfUnitsType);" +
        " SET IDENTITY_INSERT ImportedFile OFF;");

      insertedCount = dbConnection.Execute(insertCommand, importedFile);

      if (insertedCount == 1)
      {
        insertCommand = string.Format(
          "INSERT ImportedFileHistory " +
          "    (fk_ImportedFileID, CreateUTC, InsertUTC) " +
          "  VALUES " +
          "    (@LegacyImportedFileId, @FileCreatedUtc, @FileUpdatedUtc)");

        insertedCount += dbConnection.Execute(insertCommand, importedFile);
      }

      dbConnection.Close();
      return insertedCount;
    }

    protected int WriteNhOpDbCustomerAndProject(string projectDbConnectionString, ImportedFileNhOp importedFile)
    {
      var dbConnection = new SqlConnection(projectDbConnectionString);
      dbConnection.Open();

      int insertedCount = 0;
      var insertCommand = string.Format(
        $"SET IDENTITY_INSERT Customer ON;" +
        " INSERT Customer " +
        "   (ID, CustomerUID, Name, fk_CustomerTypeID, BSSID, fk_DealerNetworkID) " +
        "  VALUES (@LegacyCustomerId, @CustomerUid, @Name, 0, 'bssId', 0);" +
        "SET IDENTITY_INSERT Customer OFF;"
        );

      insertedCount = dbConnection.Execute(insertCommand, importedFile);

      if (insertedCount == 1)
      {
        insertCommand = string.Format(
          $"SET IDENTITY_INSERT Project ON;" +
          " INSERT Project " +
          "    (ID, ProjectUID, Name, fk_CustomerID, fk_ProjectTypeID, fk_SiteID, TimezoneName) " +
          "  VALUES " +
          "    (@LegacyProjectId, @ProjectUid, 'the project name', @LegacyCustomerId, 0, 0, 'whateverTZ');" +
          " SET IDENTITY_INSERT Project OFF;");
        
        insertedCount += dbConnection.Execute(insertCommand, importedFile);
      }

      dbConnection.Close();
      return insertedCount;
    }
  }
}
 