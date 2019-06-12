extern alias MySqlDataAlias;
using System;
using System.Data;
using MySqlDataAlias::MySql.Data.MySqlClient;

namespace TestUtility
{
  public static class MySqlHelper
  {
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project";
    private static readonly TestConfig _appConfig = new TestConfig(PROJECT_DB_SCHEMA_NAME);

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

      while (retryCount < 40)
      {
        resultCount = Convert.ToInt32(ExecuteRead(query));

        if (resultCount == expectedEventCount)
        {
          return;
        }

        retryCount++;

        System.Threading.Tasks.Task.Delay(500).Wait();
      }

      throw new Exception($"Expected event count {expectedEventCount} doesn't equal result count {resultCount}");
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
        if (expectedDataArray[idx].Trim() != actualDataArray[idx].Trim())
        {
          throw new Exception($"Expected array element '{expectedDataArray[idx]}' doesn't equal actual element '{actualDataArray[idx]}'");
        }

        idx++;
      }
    }

    /// <summary>
    /// Executes and SQL statement against the connection and returns the number of rows affected.
    /// </summary>
    public static int ExecuteNonQuery(string queryString)
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

    /// <summary>
    /// Read the my sql database and return a record count
    /// </summary>
    public static string ExecuteRead(string queryString)
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
