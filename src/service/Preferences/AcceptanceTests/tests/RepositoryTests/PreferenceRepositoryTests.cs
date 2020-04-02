using System;
using System.Collections.Generic;
using CCSS.Productivity3D.Preferences.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Repositories;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.Preference.Interfaces;
using Serilog;
using VSS.Common.Cache.MemoryCache;
using Dapper;
using System.Linq;

namespace RepositoryTests
{
  [TestClass]
  public class PreferenceRepositoryTests
  {
    private IServiceProvider ServiceProvider;
    private IConfigurationStore ConfigStore;
    private PreferenceRepository PrefRepo;
    
    [TestInitialize]
    public void Init()
    {
      ServiceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Filter.Repository.Tests.log")))    
        .AddTransient<IRepository<IPreferenceEvent>, PreferenceRepository>()
        .AddMemoryCache()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .BuildServiceProvider();

      ConfigStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      PrefRepo = ServiceProvider.GetRequiredService<IRepository<IPreferenceEvent>>() as PreferenceRepository;
      
      Assert.IsNotNull(ServiceProvider.GetService<ILoggerFactory>());
    }

    [TestMethod]
    public void PreferenceKeySchemaExists()
    {
      const string tableName = "PreferenceKey";
      List<string> columnNames = new List<string>
      {
        "PreferenceKeyID", "PreferenceKeyUID", "KeyName", "InsertUTC", "UpdateUTC"
      };

      CheckSchema(tableName, columnNames);
    }

    public void UserPreferenceSchemaExists()
    {
      const string tableName = "UserPreference";
      List<string> columnNames = new List<string>
      {
        "UserPreferenceID", "UserUID", "fk_PreferenceKeyID", "Value", "SchemaVersion", "InsertUTC", "UpdateUTC"
      };

      CheckSchema(tableName, columnNames);
    }

    #region privates

    [TestMethod]
    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlConnection(this.ConfigStore.GetConnectionString("VSPDB")))
      {
        try
        {
          connection.Open();

          //Check table exists
          var table = connection.Query(GetQuery(tableName, true)).FirstOrDefault();
          Assert.IsNotNull(table, "Missing " + tableName + " table schema");
          Assert.AreEqual(tableName, table.TABLE_NAME, "Wrong table name");

          //Check table columns exist
          var columns = connection.Query(GetQuery(tableName, false)).ToList();
          Assert.IsNotNull(columns, "Missing " + tableName + " table columns");
          Assert.AreEqual(columnNames.Count, columns.Count, "Wrong number of " + tableName + " columns");
          foreach (var columnName in columnNames)
            Assert.IsNotNull(columns.Find(c => c.COLUMN_NAME == columnName), "Missing " + columnName + " column in " + tableName + " table");
        }
        finally
        {
          connection.Close();
        }
      }
    }

    private string GetQuery(string tableName, bool selectTable)
    {
      string what = selectTable ? "TABLE_NAME" : "COLUMN_NAME";
      var query = string.Format("SELECT {0} FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{1}' AND TABLE_NAME ='{2}'",
        what, this.ConfigStore.GetValueString("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }

    #endregion privates
  }
}
