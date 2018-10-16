using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VSS.ConfigurationStore
{
  /// <summary>
  /// Settings come from 2 sources:
  /// environment variables and 'internal' (appsettings.json)
  /// Appsettings will override any environment setting
  /// if neither present then we'll use some defaults
  /// </summary>
  public class GenericConfiguration : IConfigurationStore
  {
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _log;

    public GenericConfiguration(ILoggerFactory logger)
    {
      IConfigurationBuilder configBuilder;
      _log = logger.CreateLogger<GenericConfiguration>();
      _log.LogTrace("GenericConfig constructing");
      var builder = configBuilder = new ConfigurationBuilder()
        .AddEnvironmentVariables();
      try
      {
        _log.LogTrace("Base:" + AppContext.BaseDirectory);
        var dirToAppsettings = Directory.GetCurrentDirectory();
        _log.LogTrace("Current:" + dirToAppsettings);
        string pathToConfigFile;

        _log.LogDebug($"Testing default path for the config file {Directory.GetCurrentDirectory()} and {AppContext.BaseDirectory}");

        //Test if appsettings exists in the default folder for the console application
        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")))
        {
          pathToConfigFile = Directory.GetCurrentDirectory();
        }
        else if (File.Exists(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
        {
          pathToConfigFile = AppContext.BaseDirectory;
        }
        else
        {
          var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
          pathToConfigFile = Path.GetDirectoryName(pathToExe);

          _log.LogTrace($"No configuration files found, using alternative path {pathToConfigFile}");
        }

        _log.LogTrace($"Using configuration file: {pathToConfigFile}");

        builder.SetBasePath(pathToConfigFile) // for appsettings.json location
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        _configuration = configBuilder.Build();
      }
      catch (Exception ex)
      {
        _log.LogCritical($"GenericConfiguration exception: {ex.Message}, {ex.Source}, {ex.StackTrace}");
        throw;
      }
    }

    public string GetConnectionString(string connectionType)
    {
      return GetConnectionString(connectionType, "MYSQL_DATABASE_NAME");
    }

    public string GetConnectionString(string connectionType, string databaseNameKey)
    {
      string serverName = null;

      switch (connectionType)
      {
        case "VSPDB":
          serverName = GetValueString("MYSQL_SERVER_NAME_VSPDB");
          break;
        case "ReadVSPDB":
          serverName = GetValueString("MYSQL_SERVER_NAME_ReadVSPDB");
          break;
      }

      var serverPort = GetValueString("MYSQL_PORT");
      var serverDatabaseName = GetValueString(databaseNameKey);
      var serverUserName = GetValueString("MYSQL_USERNAME");
      var serverPassword = GetValueString("MYSQL_ROOT_PASSWORD");

      if (serverName == null || serverPort == null || serverDatabaseName == null || serverUserName == null ||
          serverPassword == null)
      {
        var errorString =
          $"Your application is attempting to use the {connectionType} connectionType but is missing an environment variable. serverName {serverName} serverPort {serverPort} serverDatabaseName {serverDatabaseName} serverUserName {serverUserName} serverPassword {serverPassword}";
        _log.LogError(errorString);

        throw new InvalidOperationException(errorString);
      }

      var connString =
        "server=" + serverName +
        ";port=" + serverPort +
        ";database=" + serverDatabaseName +
        ";userid=" + serverUserName +
        ";password=" + serverPassword +
        ";Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4";
      _log.LogTrace($"Served connection string {connString}");

      return connString;
    }

    public string GetValueString(string key)
    {
      _log.LogTrace($"Served configuration value {key}:{_configuration[key]}");
      return _configuration[key];
    }

    public int GetValueInt(string key)
    {
      // zero is valid. Returns int.MinValue on error
      if (!int.TryParse(_configuration[key], out int valueInt))
      {
        valueInt = -1;
      }

      _log.LogTrace($"Served configuration value {key}:{valueInt}");

      return valueInt;
    }

    public bool? GetValueBool(string key)
    {
      bool? theBoolToReturn = null;
      if (bool.TryParse(_configuration[key], out bool theBool))
      {
        theBoolToReturn = theBool;
      }

      _log.LogTrace($"Served configuration value {key}:{theBoolToReturn}");

      return theBoolToReturn;
    }

    public TimeSpan? GetValueTimeSpan(string key)
    {
      TimeSpan? theTimeSpanToReturn = null;
      if (TimeSpan.TryParse(_configuration[key], out TimeSpan theTimeSpan))
      {
        theTimeSpanToReturn = theTimeSpan;
      }

      _log.LogTrace($"Served configuration value {key}:{theTimeSpanToReturn}");

      return theTimeSpanToReturn;
    }

    public IConfigurationSection GetSection(string key)
    {
      return _configuration.GetSection(key);
    }

    public IConfigurationSection GetLoggingConfig()
    {
      return GetSection("Logging");
    }
  }
}