using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using Hangfire.MySql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using Hangfire.Storage;
using MySql.Data.MySqlClient;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.TCCFileAccess;

namespace SchedulerTestsImportedFileSync
{
  public class TestControllerBase
  {
    protected IServiceProvider ServiceProvider;
    protected IConfigurationStore ConfigStore;
    protected ILoggerFactory LoggerFactory;
    protected ILogger Log;
    protected string FileSpaceId;
    protected IRaptorProxy RaptorProxy;
    protected ITPaasProxy TPaasProxy;
    protected IImportedFileProxy ImpFileProxy;
    protected IFileRepository FileRepo;
    protected DateTime _earliestHistoricalDateTime;
    protected DateTime _defaultHistoricalDateTime;
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
        .AddSingleton<IConfigurationStore, GenericConfiguration>();

      ServiceProvider = serviceCollection.BuildServiceProvider();
      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      this.LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
      Log = loggerFactory.CreateLogger<TestControllerBase>();

      FileSpaceId = ConfigStore.GetValueString("TCCFILESPACEID");
      if (string.IsNullOrEmpty(FileSpaceId))
      {
        throw new InvalidOperationException(
          "ImportedFileSynchroniser unable to establish FileSpaceId");
      }

      RaptorProxy = new RaptorProxy(ConfigStore, LoggerFactory);
      TPaasProxy = new TPaasProxy(ConfigStore, LoggerFactory);
      ImpFileProxy = new ImportedFileProxy(ConfigStore, LoggerFactory);
      FileRepo = new FileRepository(ConfigStore, LoggerFactory);

      Assert.IsNotNull(ServiceProvider.GetService<IConfigurationStore>());
      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
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


    protected bool HaveRetrievedProjectImportedFile(string projectDbConnectionString, long legacyImportedFileId)
    {
      // importedFile table depends on a project (for legacyProjectID) and CustomerProject (for legacyCustomerId) rows existing
      var dbConnection = new MySqlConnection(projectDbConnectionString);
      dbConnection.Open();

      var empty = "\"";
      string selectCommand = string.Format($"SELECT LegacyImportedFileId FROM ImportedFile WHERE LegacyImportedFileId = {empty}{legacyImportedFileId}{empty}");

      IEnumerable<object> response = null;
      response = dbConnection.Query(selectCommand);
      
      dbConnection.Close();

      var enumerable = response as IList<object> ?? response.ToList();
      return enumerable.Any();
    }

    protected int WriteNhOpDbImportedFileAndHistory(string nhOpDbConnectionString, ImportedFileNhOp importedFile)
    {
      var dbConnection = new SqlConnection(nhOpDbConnectionString);
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

      if (importedFile.FileCreatedUtc > DateTime.MinValue // not set
          && importedFile.FileUpdatedUtc > DateTime.MinValue // not set
          && insertedCount == 1)
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

    protected int WriteNhOpDbCustomerAndProject(string nhOpDbConnectionString, ImportedFileNhOp importedFile)
    {
      var dbConnection = new SqlConnection(nhOpDbConnectionString);
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

    protected bool NhOpDbCustomerAndProjectExists(string nhOpDbConnectionString, long legacyCustomerId, long legacyProjectId)
    {
      var dbConnection = new SqlConnection(nhOpDbConnectionString);
      dbConnection.Open();

      var customerId = dbConnection.QuerySingleOrDefault<long>(
        @"SELECT ID FROM Customer WHERE ID = @legacyCustomerId",
        new { legacyCustomerId });

      var projectId = dbConnection.QuerySingleOrDefault<long>(
        @"SELECT ID FROM Project WHERE ID = @legacyProjectId",
        new { legacyProjectId });

      dbConnection.Close();
      return customerId > 0 && projectId > 0;
    }
  }
}
 