using System;
using VSS.ConfigurationStore;
using Microsoft.Extensions.Logging;

namespace VSS.Productivity3D.Scheduler.Common.Utilities
{
  public static class ConnectionUtils
  {
    public static string GetConnectionStringMySql(IConfigurationStore configStore, ILogger log, string dbNameExtension)
    {
      string serverName = configStore.GetValueString("MYSQL_SERVER_NAME_VSPDB" + dbNameExtension);
      string serverPort = configStore.GetValueString("MYSQL_PORT" + dbNameExtension);
      string serverDatabaseName = configStore.GetValueString("MYSQL_DATABASE_NAME" + dbNameExtension);
      string serverUserName = configStore.GetValueString("MYSQL_USERNAME" + dbNameExtension);
      string serverPassword = configStore.GetValueString("MYSQL_ROOT_PASSWORD" + dbNameExtension);

      if (serverName == null || serverPort == null || serverDatabaseName == null || serverUserName == null ||
          serverPassword == null)
      {
        var errorString =
          $"GetConnectionStringMySql: Your application is attempting to use the {dbNameExtension} dbNameExtension but is missing an environment variable. serverName {serverName} serverPort {serverPort} serverDatabaseName {serverDatabaseName} serverUserName {serverUserName} serverPassword {serverPassword}";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }

      var connString =
        "server=" + serverName +
        ";port=" + serverPort +
        ";database=" + serverDatabaseName +
        ";userid=" + serverUserName +
        ";password=" + serverPassword +
        ";Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4";

      return connString;
    }

    public static string GetConnectionStringMsSql(IConfigurationStore configStore, ILogger log, string dbNameExtension)
    {
      // string projectDbConnectionString = string.Format($"Data Source=alpha-nh-raw.c31ahitxrkg7.us-west-2.rds.amazonaws.com;Initial Catalog=NH_OP;Integrated Security=False;User ID=root;Password=d3vRDS1234_;");
      string serverName = configStore.GetValueString("MSSQL_SERVER_NAME");
      string serverDatabaseName = configStore.GetValueString("MSSQL_DATABASE_NAME");
      string serverUserName = configStore.GetValueString("MSSQL_USERNAME");
      string serverPassword = configStore.GetValueString("MSSQL_ROOT_PASSWORD");

      if (serverName == null || serverDatabaseName == null || serverUserName == null || serverPassword == null)
      {
        var errorString =
          $"GetConnectionStringMySql: Your application is attempting to use the {dbNameExtension} dbNameExtension but is missing an environment variable. serverName {serverName} serverDatabaseName {serverDatabaseName} serverUserName {serverUserName} serverPassword {serverPassword}";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }

      var connString =
        "Data Source=" + serverName +
        "; Initial Catalog=" + serverDatabaseName +
        "; User ID=" + serverUserName +
        "; Password=" + serverPassword +
        ";";

      return connString;
    }

  }
}