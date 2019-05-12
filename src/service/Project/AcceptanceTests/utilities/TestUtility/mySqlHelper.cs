extern alias MySqlDataAlias;
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlDataAlias::MySql.Data.MySqlClient;

namespace TestUtility
{
  public class MySqlHelper
  {
    private readonly TestConfig appConfig = new TestConfig();
    private readonly Msg msg = new Msg();

    /// <summary>
    /// Read a my sql table and return records/columns 
    /// </summary>
    public string ExecuteMySqlQueryAndReturnColumns(string connectionString, string queryString, string fields)
    {
      var allFields = fields.Split(',');
      var results = string.Empty;
      using (var mySqlConnection = new MySqlConnection(connectionString))
      {
        mySqlConnection.Open();
        var mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
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
    public string ExecuteMySqlQueryAndReturnRecordCountResult(string connectionString, string queryString)
    {
      string queryResult = null;
      using (var mySqlConnection = new MySqlConnection(connectionString))
      {
        mySqlConnection.Open();
        var mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
        var mySqlDataReader = mySqlCommand.ExecuteReader();

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
    public int ExecuteMySqlInsert(string connectionString, string sqlCommand)
    {
      Console.WriteLine(sqlCommand);
      using (var mySqlConnection = new MySqlConnection(connectionString))
      {
        mySqlConnection.Open();
        var mySqlCommand = new MySqlCommand(sqlCommand, mySqlConnection);
        var result = mySqlCommand.ExecuteNonQuery();
        Console.WriteLine(result.ToString());
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
    /// Check the database to see if the records injected into kafka have reached there. This
    /// Will loop for 10 times or until it finds the correct answer.
    /// </summary>
    public int GetDatabaseCountForEvents(string query, int eventCount)
    {
      var retryCount = 0;
      var resultCount = 0;

      msg.DisplayMySqlQuery(query);
      while (retryCount < 20)
      {
        var mysqlHelper = new MySqlHelper();
        resultCount = Convert.ToInt32(mysqlHelper.ExecuteMySqlQueryAndReturnRecordCountResult(appConfig.DbConnectionString, query));
        msg.DisplayMySqlQuery(query);
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
    /// Read the database for records
    /// </summary>
    private string GetDatabaseFieldsForQuery(string query, string fields)
    {
      var mysqlHelper = new MySqlHelper();
      msg.DisplayMySqlQuery(query);
      return mysqlHelper.ExecuteMySqlQueryAndReturnColumns(appConfig.DbConnectionString, query, fields);
    }

    /// <summary>
    /// Used to update the dbSchema name which will be used when performing database actions
    /// </summary>
    public void UpdateDbSchemaName(string dbSchemaName) => appConfig.SetMySqlDbSchema(dbSchemaName);
  }
}
