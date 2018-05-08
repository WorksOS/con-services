using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Repositories.DBModels;

namespace RepositoryTests
{
  [TestClass]
  public class FilterRepositoryTests : TestControllerBase
  {

    private string _filterDbConnectionString;
    protected ILogger _log;


    [TestInitialize]
    public void Init()
    {
      SetupDI();

      _log = LoggerFactory.CreateLogger<FilterRepositoryTests>();
      _filterDbConnectionString = ConnectionUtils.GetConnectionStringMySql(ConfigStore, _log, "_FILTER");
      Assert.IsNotNull(_log, "Log is null");
    }

    [TestMethod]
    public void FilterSchemaExists_FilterTable()
    {
      const string tableName = "Filter";
      List<string> columnNames = new List<string>
      {
        "ID",
        "FilterUID",
        "fk_FilterTypeID",
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
      CheckSchema("_FILTER", tableName, columnNames);
    }

    [Ignore("This test will not work in the context as the filter repo expects MYSQL_DATABASE_NAME to point to filter not scheduler see US 69657")] 
    [TestMethod]
    public void FiltersToBeCleaned()
    {
      var dbConnection = new MySqlConnection(_filterDbConnectionString);
      dbConnection.Open();

      var filterUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var userUid = Guid.NewGuid().ToString();
      var name = "";
      var filterJson = "";
      var filterType = FilterType.Transient;
      var actionUtc = new DateTime(2017, 1, 1); // eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff
      var empty = "\"";



      string insertFilter = string.Format(
        $"INSERT Filter (FilterUID, fk_CustomerUid, fk_ProjectUID, UserID, Name, FilterJson, LastActionedUTC, fk_FilterTypeID) " +
        $"VALUES ({empty}{filterUid}{empty}, {empty}{customerUid}{empty}, {empty}{projectUid}{empty}, {empty}{userUid}{empty}, " +
        $"{empty}{filterJson}{empty}, {empty}{name}{empty}, {empty}{actionUtc.ToString($"yyyy-MM-dd HH:mm:ss.fffffff")}{empty}, {empty}{(int)filterType}{empty})");

      var insertedCount = dbConnection.Execute(insertFilter);
      try
      {
        Assert.AreEqual(1, insertedCount, "Filter Not Inserted");
        Assert.AreEqual(insertedCount, _filterRepository.GetTransientFiltersToBeCleaned(1).Result.AsList().Count);
      } 
      finally
      {
        //Clean up
        string deleteFilter = string.Format($"DELETE FROM Filter WHERE FilterUID = {empty}{filterUid}{empty};");
        dbConnection.Execute(deleteFilter);
      }

    }
  }
}
