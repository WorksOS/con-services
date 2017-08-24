using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System.Linq;
using Dapper;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RepositoryTests
{
  [TestClass]
  public class RepositoryTests : TestControllerBase
  {
    protected ILogger log;

    [TestInitialize]
    public void Init()
    {
      Console.WriteLine("in init");
      SetupDI();

      log = logger.CreateLogger<RepositoryTests>();
      Assert.IsNotNull(log, "log is null");
    }

    [TestMethod]
    public void FilterSchemaExists_FilterTable()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID",
        "FilterUID",
        "fk_CustomerUID",
        "fk_ProjectUID",
        "UserID",
        "Name",
        "FilterJson",
        "IsDeleted",
        "LastActionedUTC",
        "InsertUTC",
        "UpdateUTC"
      };
      Console.WriteLine(
        $"in FilterSchemaExists_FilterTable before checksum: tableName {tableName} columnNames {JsonConvert.SerializeObject(columnNames)} configStore {(configStore == null ? "configStoreIsNull" : "configStoreIsNotNull")} log {(log == null ? "logIsNull" : "logIsNotNull")}");
      CheckSchema("_FILTER", tableName, columnNames);
      Console.WriteLine($"exiting FilterSchemaExists_FilterTable");
    }

    [TestMethod]
    public void SchedulerSchemaExists_JobTable()
    {
      const string tableName = "Job";
      List<string> columnNames = new List<string>
      {
        "Id",
        "StateId",
        "StateName",
        "InvocationData",
        "Arguments",
        "CreatedAt",
        "ExpireAt"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_CounterTable()
    {
      const string tableName = "Counter";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "ExpireAt"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_AggregatedCounterTable()
    {
      const string tableName = "AggregatedCounter";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "ExpireAt"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_DistributedLockTable()
    {
      const string tableName = "DistributedLock";
      List<string> columnNames = new List<string>
      {
        "Resource",
        "CreatedAt"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_HashTable()
    {
      const string tableName = "Hash";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Field",
        "Value",
        "ExpireAt"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_JobParameterTable()
    {
      const string tableName = "JobParameter";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Name",
        "Value"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_JobQueueTable()
    {
      const string tableName = "JobQueue";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Queue",
        "FetchedAt",
        "FetchToken"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_JobStateTable()
    {
      const string tableName = "JobState";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Name",
        "Reason",
        "CreatedAt",
        "Data"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_ServerQueueTable()
    {
      const string tableName = "Server";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Data",
        "LastHeartbeat"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_SetQueueTable()
    {
      const string tableName = "Set";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "Score",
        "ExpireAt"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_StateQueueTable()
    {
      const string tableName = "State";
      List<string> columnNames = new List<string>
      {
        "Id",
        "JobId",
        "Name",
        "Reason",
        "CreatedAt",
        "Data"
      };
      CheckSchema("", tableName, columnNames);
    }

    [TestMethod]
    public void SchedulerSchemaExists_ListQueueTable()
    {
      const string tableName = "List";
      List<string> columnNames = new List<string>
      {
        "Id",
        "Key",
        "Value",
        "ExpireAt"
      };
      CheckSchema("", tableName, columnNames);
    }


    #region privates

    private void CheckSchema(string dbNameExtension, string tableName, List<string> columnNames)
    {
      Console.WriteLine(
        $"CheckSchema dbNameExtension {dbNameExtension} tableName {tableName} columnNames {columnNames}");

      string connectionString = string.IsNullOrEmpty(dbNameExtension)
        ? configStore.GetConnectionString("VSPDB")
        : ConnectionUtils.GetConnectionString(configStore, log, dbNameExtension);
      Console.WriteLine($"CheckSchema connectionString {connectionString}");
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
        what, configStore.GetValueString("MYSQL_DATABASE_NAME" + dbNameExtension), tableName);
      Console.WriteLine($"GetQuery query {query}");
      return query;
    }

    #endregion privates
  }
}
