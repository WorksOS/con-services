using System;
using VSS.ConfigurationStore;
using Microsoft.Extensions.Logging;

namespace VSS.Productivity3D.Scheduler.Common.Utilities
{
  public static class ConnectionUtils
  {
    public static string GetConnectionString(IConfigurationStore configStore, ILogger log, string dbNameExtension)
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
          $"GetConnectionString: Your application is attempting to use the {dbNameExtension} dbNameExtension but is missing an environment variable. serverName {serverName} serverPort {serverPort} serverDatabaseName {serverDatabaseName} serverUserName {serverUserName} serverPassword {serverPassword}";
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

 
  }
}