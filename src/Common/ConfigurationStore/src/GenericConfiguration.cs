using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VSS.ConfigurationStore
{

  internal enum KubernetesState
  {
    Initialized,
    Requested,
    NotInitialized,
    NotRequired,
    Disabled
  };

  /// <summary>
  /// Settings come from 2 sources:
  /// environment variables and 'internal' (appsettings.json)
  /// Kubernetes configuration has the lowest priority
  /// environment vars will override k8s
  /// Appsettings will override any environment setting
  /// if neither present then we'll use some defaults
  /// </summary>
  public class GenericConfiguration : IConfigurationStore
  {
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _log;


    private static Dictionary<string, string> kubernetesConfig=null;
    private static bool useKubernetes = false;
    private static string kubernetesConfigMapName = null;
    private static string kubernetesNamespace = null;
    private static string kubernetesContext = null;
    private static KubernetesState kubernetesInitialized = KubernetesState.NotInitialized;
    private object kubernetesInitLock = new object();
      

    public bool UseKubernetes
    {
      get => useKubernetes;
      set
      {
        useKubernetes = value;
        kubernetesInitialized = KubernetesState.Requested;
      }
    }

    public string KubernetesConfigMapName
    {
      get => kubernetesConfigMapName;
      private set => kubernetesConfigMapName = value;
    }

    public string KubernetesNamespace
    {
      get => kubernetesNamespace;
      private set => kubernetesNamespace = value;
    }

    public string KubernetesContext
    {
      get => kubernetesContext;
      private set => kubernetesContext = value;
    }

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

        if (kubernetesConfig != null) builder.AddInMemoryCollection(kubernetesConfig);

        _configuration = configBuilder.Build();
      }
      catch (Exception ex)
      {
        _log.LogCritical($"GenericConfiguration exception: {ex.Message}, {ex.Source}, {ex.StackTrace}");
        throw;
      }

      if (kubernetesInitialized != KubernetesState.NotInitialized)
        return;

      lock (kubernetesInitLock)
      {
        if (_configuration["UseKubernetes"] == "true" && kubernetesInitialized == KubernetesState.NotInitialized)
        {
          kubernetesInitialized = KubernetesState.Requested;
          UseKubernetes = true;
          KubernetesConfigMapName = _configuration["KubernetesConfigMapName"];
          KubernetesNamespace = _configuration["KubernetesNamespace"];
          KubernetesContext = _configuration["KubernetesNamespace"];
        }

        if (!UseKubernetes)
        {
          kubernetesInitialized = KubernetesState.NotRequired;
          return;
        }

        //try initialize Kubernetes
        if (kubernetesInitialized == KubernetesState.Requested)
        {
          try
          {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(currentContext: KubernetesContext);
            var client = new Kubernetes(config);

            kubernetesConfig = new Dictionary<string, string>(client
              .ReadNamespacedConfigMapWithHttpMessagesAsync(KubernetesConfigMapName, KubernetesNamespace).Result.Body
              .Data);
            kubernetesInitialized = KubernetesState.Initialized;
            builder.AddInMemoryCollection(kubernetesConfig);
            _configuration = configBuilder.Build();
          }
          catch (Exception ex)
          {
            _log.LogWarning(
              $"Can not connect to Kubernetes cluster with error {ex.Message}. Kubernetes is disabled for this process.");
            kubernetesInitialized = KubernetesState.Disabled;
            UseKubernetes = false;
          }
        }
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

    public string GetValueString(string key, string defaultValue)
    {
      _log.LogTrace($"Served configuration value {key}:{_configuration.GetValue<string>(key, defaultValue)}");
      return _configuration.GetValue<string>(key, defaultValue);
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

    public int GetValueInt(string key, int defaultValue)
    {
      _log.LogTrace($"Served configuration value {key}:{_configuration.GetValue<int>(key, defaultValue)}");
      return _configuration.GetValue<int>(key, defaultValue);
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


    public bool GetValueBool(string key, bool defaultValue)
    {
      _log.LogTrace($"Served configuration value {key}:{_configuration.GetValue<bool>(key, defaultValue)}");
      return _configuration.GetValue<bool>(key, defaultValue);
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

    public TimeSpan GetValueTimeSpan(string key, TimeSpan defaultValue)
    {
      _log.LogTrace($"Served configuration value {key}:{_configuration.GetValue<TimeSpan>(key, defaultValue)}");
      return _configuration.GetValue<TimeSpan>(key, defaultValue);
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