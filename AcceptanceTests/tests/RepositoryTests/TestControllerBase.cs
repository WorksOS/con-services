using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace RepositoryTests
{
  public class TestControllerBase
  {
    protected ILoggerFactory LoggerFactory;
    protected IConfigurationStore ConfigStore;

    private IServiceProvider _serviceProvider;
    private ILogger _log;

    public void SetupDI()
    {
      const string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
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

      _serviceProvider = serviceCollection.BuildServiceProvider();
      ConfigStore = _serviceProvider.GetRequiredService<IConfigurationStore>();
      this.LoggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
      _log = loggerFactory.CreateLogger<TestControllerBase>();

      Assert.IsNotNull(_serviceProvider.GetService<IConfigurationStore>());
      Assert.IsNotNull(_serviceProvider.GetService<ILoggerFactory>());
    }


    #region schema

    public void CheckSchema(string dbNameExtension, string tableName, List<string> columnNames)
    {
      string connectionString = string.IsNullOrEmpty(dbNameExtension)
        ? ConfigStore.GetConnectionString("VSPDB")
        : ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, dbNameExtension);

      using (var connection = new MySqlConnection(connectionString))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(dbNameExtension, tableName, true)).FirstOrDefault();

          Assert.IsNotNull(table, "Missing " + tableName + " table schema");
          Assert.AreEqual(tableName, table.TABLE_NAME, "Wrong table name");

          //Check table columns exist
          var columns = connection.Query(GetQuery(dbNameExtension, tableName, false)).ToList();
          Assert.IsNotNull(columns, "Missing " + tableName + " table columns");
          Assert.AreEqual(columnNames.Count, columns.Count, "Wrong number of " + tableName + " columns");
          foreach (var columnName in columnNames)
            Assert.IsNotNull(columns.Find(c => c.COLUMN_NAME == columnName),
              "Missing " + columnName + " column in " + tableName + " table");
        }
        finally
        {
          connection.Close();
        }
      }
    }

    private string GetQuery(string dbNameExtension, string tableName, bool selectTable)
    {
      string what = selectTable ? "TABLE_NAME" : "COLUMN_NAME";
      var query = string.Format(
        "SELECT {0} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME ='{2}'",
        what, ConfigStore.GetValueString("MYSQL_DATABASE_NAME" + dbNameExtension), tableName);
      return query;
    }

    #endregion schema
  }
}
 