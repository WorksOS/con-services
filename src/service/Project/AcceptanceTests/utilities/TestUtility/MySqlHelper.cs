extern alias MySqlDataAlias;
using System;
using System.Data;
using System.Threading;
using MySqlDataAlias::MySql.Data.MySqlClient;
using Xunit;

namespace TestUtility
{
  public static class MySqlHelper
  {
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project";

    private static readonly TestConfig _appConfig = new TestConfig(PROJECT_DB_SCHEMA_NAME);

    private static readonly object _databaseLock = new object();
    private static readonly object _legacyIdLock = new object();

    private static int _currentLegacyProjectId;

    static MySqlHelper()
    {
      const string query = "SELECT max(LegacyProjectID) FROM Project WHERE LegacyProjectID < 100000;";

      var result = ExecuteMySqlQueryAndReturnRecordCountResult(query);
      var index = string.IsNullOrEmpty(result)
        ? 1000
        : Convert.ToInt32(result);

      _currentLegacyProjectId = Math.Max(index, _currentLegacyProjectId);
    }

    public static int GenerateLegacyProjectId()
    {
      lock (_legacyIdLock)
      {
        _currentLegacyProjectId += 1;

        return _currentLegacyProjectId;
      }
    }

    /// <summary>
    /// Read the my sql database and return a record count
    /// </summary>
    public static string ExecuteMySqlQueryAndReturnRecordCountResult(string queryString)
    {
      lock (_databaseLock)
      {
        string queryResult = null;

        using (var mySqlConnection = new MySqlConnection(_appConfig.DbConnectionString))
        {
          mySqlConnection.Open();

          using (var mySqlCommand = new MySqlCommand(queryString, mySqlConnection))
          using (var mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
          {
            while (mySqlDataReader.Read())
            {
              queryResult = mySqlDataReader[0].ToString();
            }

            return queryResult;
          }
        }
      }
    }

    /// <summary>
    /// Used for updating the database
    /// </summary>
    public static int ExecuteMySqlInsert(string queryString)
    {
      lock (_databaseLock)
      {
        Console.WriteLine(queryString);

        using (var mySqlConnection = new MySqlConnection(_appConfig.DbConnectionString))
        {
          mySqlConnection.Open();

          using (var mySqlCommand = new MySqlCommand(queryString, mySqlConnection))
          {
            var result = mySqlCommand.ExecuteNonQuery();

            Console.WriteLine(result.ToString());

            return result;
          }
        }
      }
    }

    /// <summary>
    /// Verify the number of expected records in the table is there for the given uid
    /// </summary>
    /// <remarks>
    /// Checks the database to see if the records injected into kafka have reached there.
    /// This will loop until it finds the correct answer or exceeds the retry count.
    /// </remarks>
    public static void VerifyTestResultDatabaseRecordCount(string tableName, string columnName, int expectedEventCount, Guid uid)
    {
      var query = $"SELECT COUNT(*) FROM `{_appConfig.dbSchema}`.{tableName} WHERE {columnName}='{uid}'";
      var retryCount = 0;
      var resultCount = 0;

      Msg.DisplayMySqlQuery(query);

      while (retryCount < 20)
      {
        resultCount = Convert.ToInt32(ExecuteMySqlQueryAndReturnRecordCountResult(query));

        if (resultCount == expectedEventCount)
        {
          break;
        }

        retryCount++;

        Thread.Sleep(1000);
      }

      Assert.Equal(expectedEventCount, resultCount);
    }

    /// <summary>
    /// Verify the value of fields in the table for the given uid
    /// </summary>
    public static void VerifyTestResultDatabaseFieldsAreExpected(string tableName, string columnName, string fields, string expectedData, Guid uid)
    {
      var allActualData = GetDatabaseFieldsForQuery(
        $"SELECT {fields} FROM `{_appConfig.dbSchema}`.{tableName} WHERE {columnName}='{uid}'",
        fields);

      var fldArray = fields.Split(',');
      var actualDataArray = allActualData.Split(',');
      var expectedDataArray = expectedData.Split(',');
      var idx = 0;
      Msg.DisplayResults(expectedData, allActualData);

      foreach (var _ in fldArray)
      {
        Assert.Equal(expectedDataArray[idx].Trim(), actualDataArray[idx].Trim());
        idx++;
      }
    }

    /// <summary>
    /// Read the database for records
    /// </summary>
    private static string GetDatabaseFieldsForQuery(string queryString, string fields)
    {
      var allFields = fields.Split(',');
      var results = string.Empty;

      using (var mySqlConnection = new MySqlConnection(_appConfig.DbConnectionString))
      {
        mySqlConnection.Open();

        using (var mySqlCommand = new MySqlCommand(queryString, mySqlConnection))
        using (var mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
        {
          while (mySqlDataReader.Read())
          {
            foreach (var col in allFields)
            {
              results = results + mySqlDataReader[col.Trim()] + ",";
            }
          }

          return results.TrimEnd(',');
        }
      }
    }
  }
}
