using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TestUtility
{
  public class MySqlHelper
  {
    private readonly TestConfig appConfig = new TestConfig();
    private readonly Msg msg = new Msg();
    public readonly TestConfig TsCfg = new TestConfig();

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
     // msg.DisplayResults(expectedEventCount + " records", result + " records");
      Assert.AreEqual(expectedEventCount, result, " Number of expected events do not match actual events in database");
    }

    public string VerifyProjectGeofence(string projectUid, int expectedEventCount)
    {
      // since we're using geofenceProxy which doesn't write to db, we cant check Geofence in DB
      var sqlQuery = $@"SELECT COUNT(*) 
                          FROM Project p 
                            INNER JOIN ProjectGeofence pg ON pg.fk_ProjectUID = p.ProjectUID                           
                          WHERE ProjectUID = '{projectUid}'";
      var result = GetDatabaseCountForEvents(sqlQuery, expectedEventCount);
      Assert.AreEqual(expectedEventCount, result, " Number of expected events do not match actual events in database");

      string geofenceUid = null;
      if (result == 1)
      {
        sqlQuery = $@"SELECT pg.fk_GeofenceUID
                          FROM Project p 
                            INNER JOIN ProjectGeofence pg ON pg.fk_ProjectUID = p.ProjectUID                           
                          WHERE ProjectUID = '{projectUid}'";
        geofenceUid = ExecuteMySqlQueryAndReturnRecordCountResult(TsCfg.DbConnectionString, sqlQuery);
      }
      return geofenceUid;
    }
    
    /// <summary>
    /// Verify the value of fields in the table for the given uid
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
    /// Verify the value of fields in the table for the given uid (not equal)
    /// </summary>
    /// <param name="table">Database table name</param>
    /// <param name="column">Database column name for the uid</param>
    /// <param name="fields"></param>
    /// <param name="expectedData"></param>
    /// <param name="uid">The uid to use</param>
    public void VerifyTestResultDatabaseFieldsAreNotEqualAsExpected(string table, string column, string fields, string expectedData, Guid uid)
    {
      var sqlQuery = @"SELECT {4} FROM `{0}`.{1} WHERE {2}='{3}'";
      var allActualData = GetDatabaseFieldsForQuery(string.Format(sqlQuery, appConfig.dbSchema, table, column, uid, fields), fields);
      var fldArray = fields.Split(',');
      var actualDataArray = allActualData.Split(',');
      var expectedDataArray = expectedData.Split(',');
      var idx = 0;
      msg.DisplayResults(expectedData, allActualData);
      foreach (var col in fldArray)
      {
        Assert.AreNotEqual(expectedDataArray[idx].Trim(), actualDataArray[idx].Trim(), "Expected results for " + col + " do not match actual");
        idx++;
      }
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
      while (retryCount < 30)
      {
        var mysqlHelper = new MySqlHelper();
        resultCount = Convert.ToInt32(mysqlHelper.ExecuteMySqlQueryAndReturnRecordCountResult(appConfig.DbConnectionString, query));
        //msg.DisplayMySqlQuery(query);
        if (resultCount == eventCount)
        {
          break;
        }
        retryCount++;
        Thread.Sleep(500);
      }
      return resultCount;
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
