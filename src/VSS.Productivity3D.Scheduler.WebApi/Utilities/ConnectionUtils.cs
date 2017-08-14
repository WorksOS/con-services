  using System;
  using Microsoft.Extensions.Configuration;
  using MySql.Data.MySqlClient;
  using VSS.ConfigurationStore;

namespace VSS.Productivity3D.Scheduler.WebAPI.Utilities
{
  public static class ConnectionUtils
  {
    private const string DatabaseVariable = "Hangfire_SqlServer_DatabaseName";
    private const string ConnectionStringTemplateVariable = "Hangfire_SqlServer_ConnectionStringTemplate";
    private const string FilterDatabaseName = "VSS-Productivity3D-Filter";

    private const string MasterDatabaseName = "mysql";
    private const string DefaultDatabaseName = "VSS-Hangfire";

    private const string DefaultConnectionStringTemplate
      = "server=127.0.0.1;uid=root;pwd=abc123;database={0};Allow User Variables=True";

    public static string GetConnectionString(string connectionType)
    {
      string serverName = null;
      string serverPort = null;
      string serverDatabaseName = null;
      string serverUserName = null;
      string serverPassword = null;

      if (connectionType == "Scheduler")
      {
        serverName = "localhost"; // configStore.GetValueString("MYSQL_SERVER_NAME");
        serverPort = "3306";
        serverDatabaseName = "VSS-Productivity3D-Scheduler";
        serverUserName = "root";
        serverPassword = "abc123";
      }
      else // "Filter":
      {
        serverName = "localhost"; // configStore.GetValueString("MYSQL_SERVER_NAME_FILTER");
          serverPort = "3306";
          serverDatabaseName = "VSS-Productivity3D-Filter";
          serverUserName = "root";
          serverPassword = "abc123";
      }

      //var serverPort = configStore.GetValueString("MYSQL_PORT");
      //var serverDatabaseName = configStore.GetValueString("MYSQL_DATABASE_NAME");
      //var serverUserName = configStore.GetValueString("MYSQL_USERNAME");
      //var serverPassword = configStore.GetValueString("MYSQL_ROOT_PASSWORD");

      if (serverName == null || serverPort == null || serverDatabaseName == null || serverUserName == null ||
          serverPassword == null)
      {
        var errorString =
          $"Your application is attempting to use the {connectionType} connectionType but is missing an environment variable. serverName {serverName} serverPort {serverPort} serverDatabaseName {serverDatabaseName} serverUserName {serverUserName} serverPassword {serverPassword}";
        //_log.LogError(errorString);

        throw new InvalidOperationException(errorString);
      }

      var connString =
        "server=" + serverName +
        ";port=" + serverPort +
        ";database=" + serverDatabaseName +
        ";userid=" + serverUserName +
        ";password=" + serverPassword +
        ";Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4";
      //_log.LogTrace($"Served connection string {connString}");

      return connString;
    }

    public static string GetDatabaseName()
    {
      return Environment.GetEnvironmentVariable(DatabaseVariable) ?? DefaultDatabaseName;
    }

    public static string GetMasterConnectionString()
    {
      return String.Format(GetConnectionStringTemplate(), MasterDatabaseName);
    }

    public static string GetHangfireConnectionString()
    {
      return String.Format(GetConnectionStringTemplate(), GetDatabaseName());
    }

    public static string GetFilterConnectionString()
    {
      return String.Format(GetConnectionStringTemplate(), FilterDatabaseName);
    }

    private static string GetConnectionStringTemplate()
    {
      return Environment.GetEnvironmentVariable(ConnectionStringTemplateVariable)
             ?? DefaultConnectionStringTemplate;
    }

    public static MySqlConnection CreateHangfireConnection()
    {
      var connection = new MySqlConnection(GetHangfireConnectionString());
      connection.Open();

      return connection;
    }

    public static MySqlConnection CreateFilterConnection()
    {
      var connection = new MySqlConnection(GetConnectionString("Filter"));
      connection.Open();

      return connection;
    }
  }
}
