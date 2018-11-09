using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;

namespace TestUtility
{
  public class MySqlHelper
  {
    private readonly TestConfig appConfig = new TestConfig();
    private readonly Msg msg = new Msg();

    /// <summary>
    /// Read a my sql table and return records/columns 
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="queryString"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public string ExecuteMySqlQueryAndReturnColumns(string connectionString, string queryString, string fields)
    {
      var allFields = fields.Split(',');
      var results = string.Empty;
      using (var mySqlConnection = new MySqlConnection(connectionString))
      {
        mySqlConnection.Open();
        MySqlCommand mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
        using (var mySqlDataReader = mySqlCommand.ExecuteReader())
        {
          while (mySqlDataReader.Read())
          {
            foreach (var col in allFields)
            {
              results = results + mySqlDataReader[col.Trim()] + ",";
            }
          }
          results = results.TrimEnd(',');
        }
      }
      return results;
    }

    /// <summary>
    /// Read the my sql database and return a record count
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="queryString"></param>
    /// <returns></returns>
    public string ExecuteMySqlQueryAndReturnRecordCountResult(string connectionString, string queryString)
    {
      string queryResult = null;
      using (var mySqlConnection = new MySqlConnection(connectionString))
      {
        mySqlConnection.Open();
        MySqlCommand mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
        MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();

        while (mySqlDataReader.Read())
        {
          queryResult = mySqlDataReader[0].ToString();
        }
      }
      return queryResult;
    }

    /// <summary>
    /// Used for updating the database
    /// </summary>
    /// <param name="connectionString">Connection string</param>
    /// <param name="sqlCommand">SQL command to execute</param>
    /// <returns>Count of records effected</returns>
    public int ExecuteMySqlInsert(string connectionString, string sqlCommand)
    {
      Log.Info(sqlCommand, Log.ContentType.DbQuery);
      using (var mySqlConnection = new MySqlConnection(connectionString))
      {
        mySqlConnection.Open();
        var mySqlCommand = new MySqlCommand(sqlCommand, mySqlConnection);
        var result = mySqlCommand.ExecuteNonQuery();
        Log.Info(result.ToString(), Log.ContentType.DbQuery);
        return result;
      }
    }

    /// <summary>
    /// Verify the number of expected records in the table is there for the given uid
    /// </summary>
    /// <param name="table">Database table name</param>
    /// <param name="column">Database column name for the uid</param>
    /// <param name="expectedEventCount">Number of expected events</param>
    /// <param name="uid">The uid to use</param>
    public void VerifyTestResultDatabaseRecordCount(string table, string column, int expectedEventCount, Guid uid)
    {
      var sqlQuery = @"SELECT COUNT(*) FROM `{0}`.{1} WHERE {2}='{3}'";
      var result = GetDatabaseCountForEvents(string.Format(sqlQuery, appConfig.dbSchema, table, column, uid), expectedEventCount);
      msg.DisplayResults(expectedEventCount + " records", result + " records");
      Assert.AreEqual(expectedEventCount, result, " Number of expected events do not match actual events in database");
    }

    /// <summary>
    /// Verify the number of expected records in the table is there for the given uid
    /// </summary>
    /// <param name="table">Database table name</param>
    /// <param name="column">Database column name for the uid</param>
    /// <param name="fields"></param>
    /// <param name="expectedData"></param>
    /// <param name="uid">The uid to use</param>
    public void VerifyTestResultDatabaseFieldsAreExpected(string table, string column, string fields, string expectedData, Guid uid)
    {
      var sqlQuery = @"SELECT {4} FROM `{0}`.{1} WHERE {2}='{3}'";
      var allActualData = GetDatabaseFieldsForQuery(string.Format(sqlQuery, appConfig.dbSchema, table, column, uid, fields), fields);
      var fldArray = fields.Split(',');
      var actualDataArray = allActualData.Split(',');
      var expectedDataArray = expectedData.Split(',');
      var idx = 0;
      msg.DisplayResults(expectedData, allActualData );
      foreach (var col in fldArray)
      {
        Assert.AreEqual(expectedDataArray[idx].Trim(), actualDataArray[idx].Trim(), "Expected results for " + col + " do not match actual");
        idx++;
      }
    }


    /// <summary>
    /// Create a Asset Utc offset 
    /// </summary>
    /// <param name="utcOffSetHours">utc off set in hours</param>
    public void CreateAssetUtcOffset(double utcOffSetHours, string AssetUid)
    {
      var query = $@"INSERT INTO `{appConfig.dbSchema}`.{"AssetUTCOffset"} (AssetUID,EventUTC,UTCOffsetMinutes) VALUES
                        ('{AssetUid}','2015-01-01 00:00:00.000000',{utcOffSetHours * 60});";
      var mysqlHelper = new MySqlHelper();
      mysqlHelper.ExecuteMySqlInsert(appConfig.DbConnectionString, query);
    }


    /// <summary>
    /// Check the database to see if the records injected into kafka have reached there. This
    /// Will loop for 20 times or until it finds the correct answer.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="eventCount"></param>
    /// <returns>Count of records</returns>
    public int GetDatabaseCountForEvents(string query, int eventCount)
    {
      var retryCount = 0;
      var resultCount = 0;

      msg.DisplayMySqlQuery(query);
      while (retryCount < 20)
      {
        var mysqlHelper = new MySqlHelper();
        resultCount = Convert.ToInt32(mysqlHelper.ExecuteMySqlQueryAndReturnRecordCountResult(appConfig.DbConnectionString, query));
        if (resultCount == eventCount)
        {
          break;
        }
        retryCount++;
        Thread.Sleep(1000);
      }
      return resultCount;
    }

    /// <summary>
    /// Get Database Event Device Time
    /// </summary>
    /// <param name="assetUid">Asset id</param>
    /// <param name="eventUtc">event utc date time</param>
    /// <param name="table">database table</param>
    /// <returns>event device time</returns>
    public DateTime GetDatabaseEventDeviceTime(string assetUid, DateTime eventUtc, string table, string AssetUid)
    {
      string query = $@"SELECT EventDeviceTime FROM `{appConfig.dbSchema}`.{table} WHERE AssetUID = '{AssetUid}' AND EventUTC = '{eventUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}'";
      msg.DisplayMySqlQuery(query);
      Thread.Sleep(5000); // Delay so there is enough time to do this query
      var mysqlHelper = new MySqlHelper();
      string eventDeviceTime = mysqlHelper.ExecuteMySqlQueryAndReturnColumns(appConfig.DbConnectionString, query, "EventDeviceTime");
      Console.WriteLine(string.Format($@"'EventDeviceTime' in the DB = '{eventDeviceTime}'"));
      DateTime result = new DateTime();
      try
      {
        result = Convert.ToDateTime(eventDeviceTime);
      }
      catch (Exception)
      {
        Assert.Fail($@"Unable to convert EventDeviceTime to DateTime or cannot find event in {table}.");
      }

      return result;
    }

    /// <summary>
    /// Create a last reported date time in the assetbookmark table
    /// </summary>
    /// <param name="assetUid">Asset Uid</param>
    /// <param name="lastEventDate">DateTime of the last event</param>
    public void CreateDateLastReported(string assetUid, DateTime lastEventDate)
    {
      var assetBookmarksquery = $@"INSERT INTO `{appConfig.dbSchema}`.{"AssetBookmarks"} 
                        (AssetUID,LastReportedEventUTC) VALUES ('{assetUid}','{lastEventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      var mysqlHelper = new MySqlHelper();
      mysqlHelper.ExecuteMySqlInsert(appConfig.DbConnectionString, assetBookmarksquery);
    }


    /// <summary>
    /// Get the Utc offset hours 
    /// </summary>
    /// <param name="assetUid"></param>
    /// <returns></returns>
    public List<int> GetAssetUtcOffsetHours(string assetUid)
    {
      string query = $@"SELECT UTCOffsetMinutes FROM `{appConfig.dbSchema}`.AssetUTCOffset WHERE AssetUID = '{assetUid}'";
      msg.DisplayMySqlQuery(query);
      var mysqlHelper = new MySqlHelper();
      string utcOffsetMinutes = mysqlHelper.ExecuteMySqlQueryAndReturnColumns(appConfig.DbConnectionString, query, "UTCOffsetMinutes");
      Console.WriteLine($@"'UTCOffsetMinutes' in the DB = '{utcOffsetMinutes}'");

      List<int> result = new List<int>();
      foreach (string offset in utcOffsetMinutes.Split(','))
      {
        try
        {
          int offsetHrs = Convert.ToInt32(offset) / 60;
          result.Add(offsetHrs);
        }
        catch (Exception)
        {
          Assert.Fail(string.Format("No UTCOffsetMinutes record found for '{0}'", assetUid));
        }
      }

      return result;
    }

    /// <summary>
    /// Get value of column 'LastReportedEventUTC' for an asset from the database.
    /// </summary>
    /// <param name="assetUid"></param>
    /// <returns></returns>
    public DateTime GetDatabaseLastReportedEventUtc(string assetUid)
    {
      string query = $@"SELECT LastReportedEventUTC FROM `{appConfig.dbSchema}`.AssetBookmarks WHERE AssetUID = '{assetUid}'";
      msg.DisplayMySqlQuery(query);
      var mysqlHelper = new MySqlHelper();
      string lastReportedEventUtc = mysqlHelper.ExecuteMySqlQueryAndReturnColumns(appConfig.DbConnectionString, query, "LastReportedEventUTC");
      Console.WriteLine($@"'LastReportedEventUTC' in the DB = '{lastReportedEventUtc}'");

      DateTime result = new DateTime();
      try
      {
        result = Convert.ToDateTime(lastReportedEventUtc);
      }
      catch (Exception)
      {
        Assert.Fail($@"Unable to convert LastReportedEventUTC value '{lastReportedEventUtc}'to DateTime.");
      }

      return result;
    }



    /// <summary>
    /// Read the database for records
    /// </summary>
    /// <param name="query"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    private string GetDatabaseFieldsForQuery(string query, string fields)
    {
      var mysqlHelper = new MySqlHelper();
      msg.DisplayMySqlQuery(query);
      return mysqlHelper.ExecuteMySqlQueryAndReturnColumns(appConfig.DbConnectionString, query, fields);
    }

    /// <summary>
    /// Used to update the dbSchema name which will be used when performing database actions
    /// </summary>
    /// <param name="dbSchemaName"></param>
    public void UpdateDbSchemaName(string dbSchemaName)
    {
      appConfig.SetMySqlDbSchema(dbSchemaName);
    }

  }
}
