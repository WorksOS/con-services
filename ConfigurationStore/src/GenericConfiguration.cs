using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// Settings come from 2 sources:
/// environment variables and 'internal' (appsettings.json)
/// Appsettings will override any environment setting
/// if neither present then we'll use some defaults
/// </summary>

namespace VSS.GenericConfiguration
{
  public class GenericConfiguration : IConfigurationStore
  {
    //private static readonly ILogger log = serviceProvider.GetService<ILoggerFactory>().CreateLogger<ConfigSettings>();
    private IConfigurationBuilder configBuilder = null;

    private IConfigurationRoot configuration = null;
    private readonly ILogger log;

    public GenericConfiguration(ILoggerFactory logger)
    {
      log = logger.CreateLogger<GenericConfiguration>();
      log.LogTrace("GenericConfig constructing");
      var builder = configBuilder = new ConfigurationBuilder()
        .AddEnvironmentVariables();
      try
      {
        log.LogTrace("Base:" + System.AppContext.BaseDirectory);
        var dirToAppsettings = System.IO.Directory.GetCurrentDirectory();
        log.LogTrace("Current:" + dirToAppsettings);
        var pathToConfigFile = string.Empty;

        log.LogDebug($"Testing default path for the config file {System.IO.Directory.GetCurrentDirectory()}");
        //Test if appsettings exists in the default folder for the console application
        if (File.Exists($"{System.IO.Directory.GetCurrentDirectory()}\\appsettings.json"))
          pathToConfigFile = System.IO.Directory.GetCurrentDirectory();
        else
        {
          var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
          pathToConfigFile = Path.GetDirectoryName(pathToExe);
          log.LogDebug($"Setting alternative path for the config file {pathToConfigFile}");
        }

        builder.SetBasePath(pathToConfigFile) // for appsettings.json location
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration = configBuilder.Build();
      }
      catch (Exception ex)
      {
        log.LogCritical("GenericConfiguration exception: {0}, {1}, {2}", ex.Message, ex.Source, ex.StackTrace);
        throw;
      }
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

      if (serverName == null || serverPort == null || serverDatabaseName == null || serverUserName == null ||
          serverPassword == null)
      {
        var errorString =
          $"Your application is attempting to use the {connectionType} connectionType but is missing an environment variable. serverName {serverName} serverPort {serverPort} serverDatabaseName {serverDatabaseName} serverUserName {serverUserName} serverPassword {serverPassword}";
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
      log.LogDebug($"Served connection string {connString}");

      return connString;
    }

    public string GetValueString(string key)
    {
      log.LogDebug($"Served configuration value {key}:{configuration[key]}");
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
      log.LogDebug($"Served configuration value {key}:{theInt}");
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
      log.LogDebug($"Served configuration value {key}:{theBoolToReturn}");

      return theBoolToReturn;
    }

  }
}
