using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
//using VSS.Project.Service;
using VSS.Project.Service.Utils;

namespace RepositoryTests
{
  [TestClass]
  public class SchemaTests
  {
   [TestMethod]
    public void AssetSchemaExists()
    {
      const string tableName = "Asset";
      List<string> columnNames = new List<string>
          {
            "AssetUID", "Name" , "MakeCode" , "SerialNumber", "Model", "IconKey", "AssetType", "IsDeleted", "LastActionedUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void AssetConfigSchemaExists()
    {
      const string tableName = "AssetConfiguration";
      List<string> columnNames = new List<string>
          {
            "AssetUID", "StartKeyDate", "LoadSwitchNumber", "LoadSwitchWorkStartState", "DumpSwitchNumber", "DumpSwitchWorkStartState", "TargetCyclesPerDay", "VolumePerCycleCubicMeter", "InsertUTC"
          };
      CheckSchema(tableName, columnNames);
    }

      [TestMethod]
      public void OdometerSchemaExists()
      {
          const string tableName = "OdometerMeterEvent";
          List<string> columnNames = new List<string>
          {
              "ID",
              "AssetUID",
              "OdometerMeter",
              "EventUTC",
              "EventDeviceTime",
              "EventKeyDate",
              "InsertUTC",
              "UpdateUTC",
          };
          CheckSchema(tableName, columnNames);
      }


      [TestMethod]
    public void AssetUtcOffsetSchemaExists()
    {
      const string tableName = "AssetUTCOffset";
      List<string> columnNames = new List<string>
          {
            "AssetUID", "EventUTC", "UTCOffsetMinutes" , "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    [TestMethod]
    public void AssetBookmarksSchemaExists()
    {
      const string tableName = "AssetBookmarks";
      List<string> columnNames = new List<string>
          {
            "AssetUID", "LastReportedEventUTC", "InsertUTC", "UpdateUTC"
          };
      CheckSchema(tableName, columnNames);
    }

    private void CheckSchema(string tableName, List<string> columnNames)
    {
      using (var connection = new MySqlConnection(new GenericConfiguration().GetConnectionString("VSPDB")))
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
        what, Environment.GetEnvironmentVariable("MYSQL_DATABASE_NAME"), tableName);
      return query;
    }


  }

}
