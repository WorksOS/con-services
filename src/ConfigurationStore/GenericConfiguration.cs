using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
///  Settings come from 2 sources:
//    environment variables and 'internal' (appsettings.json)
//   Appsettings will override any environment setting
//   if neither present then we'll use some defaults
/// </summary>

namespace VSS.Project.Service.Utils
{
  public class GenericConfiguration : IConfigurationStore
  {


    private IConfigurationBuilder configBuilder = null;
    private IConfigurationRoot configuration = null;



    public GenericConfiguration()
    {
      var builder = configBuilder = new ConfigurationBuilder()
          .AddEnvironmentVariables();
      try
      {

        builder.SetBasePath(System.AppContext.BaseDirectory) // for appsettings.json location
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration = configBuilder.Build();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }

    public bool Init()
    {
      var log = DependencyInjectionProvider.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<GenericConfiguration>();
      log.LogInformation("KAFKA_URI: " + GetValueString("KAFKA_URI"));
      log.LogInformation("KAFKA_PORT: " + GetValueInt("KAFKA_PORT"));
      log.LogInformation("KAFKA_GROUP_NAME: " + GetValueString("KAFKA_GROUP_NAME"));
      log.LogInformation("KAFKA_STACKSIZE: " + GetValueInt("KAFKA_STACKSIZE"));
      log.LogInformation("KAFKA_AUTO_COMMIT: " + GetValueBool("KAFKA_AUTO_COMMIT"));
      log.LogInformation("KAFKA_OFFSET: " + GetValueString("KAFKA_OFFSET"));
      log.LogInformation("KAFKA_TOPIC_NAME_SUFFIX: " + GetValueString("KAFKA_TOPIC_NAME_SUFFIX"));
      log.LogInformation("KAFKA_POLL_PERIOD: " + GetValueInt("KAFKA_POLL_PERIOD"));
      log.LogInformation("KAFKA_BATCH_SIZE: " + GetValueInt("KAFKA_BATCH_SIZE"));

      if (GetValueString("KAFKA_TOPIC_NAME_SUFFIX") == null
           || GetValueString("KAFKA_URI") == null
           || GetValueInt("KAFKA_PORT") == int.MinValue
           || GetValueString("KAFKA_GROUP_NAME") == null
           || GetValueInt("KAFKA_STACKSIZE") == int.MinValue
           || GetValueBool("KAFKA_AUTO_COMMIT") == null
           || GetValueString("KAFKA_OFFSET") == null
           || GetValueInt("KAFKA_POLL_PERIOD") == int.MinValue
           || GetValueInt("KAFKA_BATCH_SIZE") == int.MinValue
          )
        return false;

      return true;
    }

    public string GetConnectionString(string connectionType)
    {
      string serverName = null;
      if (connectionType == "VSPDB")
        serverName = GetValueString("MYSQL_SERVER_NAME_VSPDB");
      else if (connectionType == "ReadVSPDB")
        serverName = GetValueString("MYSQL_SERVER_NAME_ReadVSPDB");

      var serverPort = GetValueString("MYSQL_PORT");
      var serverDatabaseName = GetValueString("MYSQL_DATABASE_NAME");
      var serverUserName = GetValueString("MYSQL_USERNAME");
      var serverPassword = GetValueString("MYSQL_ROOT_PASSWORD");

      /*  log.LogInformation("MYSQL_SERVER_NAME_VSPDB" + serverName);
        log.LogInformation("MYSQL_PORT" + serverPort);
        log.LogInformation("MYSQL_DATABASE_NAME" + serverDatabaseName);
        log.LogInformation("MYSQL_USERNAME" + serverUserName);
        log.LogInformation("MYSQL_ROOT_PASSWORD" + serverPassword);*/

      if (serverName == null || serverPort == null || serverDatabaseName == null || serverUserName == null || serverPassword == null)
      {
        var errorString = string.Format(
              "Your application is attempting to use the {0} connectionType but is missing an environment variable. serverName {1} serverPort {2} serverDatabaseName {3} serverUserName {4} serverPassword {5}",
              connectionType, serverName, serverPort, serverDatabaseName, serverUserName, serverPassword);
        //log.LogError(errorString);
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

    public string GetValueString(string key)
    {
      return configuration[key];
    }

    public int GetValueInt(string key)
    {
      // zero is valid. Returns int.MinValue on error
      int theInt;
      if (!int.TryParse(configuration[key], out theInt))
      {
        theInt = -1;
      }
      return theInt;
    }

    public bool? GetValueBool(string key)
    {
      // zero is valid. Returns int.MinValue on error
      bool? theBoolToReturn = null;
      bool theBool;
      if (bool.TryParse(configuration[key], out theBool))
      {
        theBoolToReturn = theBool;
      }
      return theBoolToReturn;
    }

  }
}
