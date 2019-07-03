using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;

namespace VSS.ConfigurationStore
{
  /// <summary>
  /// Settings come from 3 sources:
  /// environment variables and 'internal' (appsettings.json)
  /// Kubernetes configuration has the lowest priority
  /// environment vars will override k8s
  /// Appsettings will override any environment setting
  /// if neither present then we'll use some defaults
  /// Also kubernetes is supported as one of the sources with the lowest priority
  /// </summary>
  public class GenericConfiguration : IConfigurationStore
  {
    private const string APP_SETTINGS_FILENAME = "appsettings.json";
    private readonly IConfigurationRoot configuration;
    private readonly ILogger log;

    private static Dictionary<string, string> kubernetesConfig;
    private static bool useKubernetes;
    private static string kubernetesConfigMapName;
    private static string kubernetesNamespace;
    private static string kubernetesContext;
    private static KubernetesState kubernetesInitialized = KubernetesState.NotInitialized;
    private readonly object kubernetesInitLock = new object();
    private static bool configListed = false;

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
      log = logger.CreateLogger<GenericConfiguration>();
      if (log.IsTraceEnabled())
        log.LogTrace("GenericConfig constructing");

      if (kubernetesInitialized == KubernetesState.NotInitialized)
      {
        log.LogDebug("Initializing Kubernetes plugin");
        InitKubernetes();
      }

      var builder = configBuilder = new ConfigurationBuilder();
      if (kubernetesConfig != null) builder.AddInMemoryCollection(kubernetesConfig);
      builder.AddEnvironmentVariables();


      try
      {
        var pathToConfigFile = PathToConfigFile();
        if (log.IsTraceEnabled())
            log.LogTrace($"Using configuration file: {pathToConfigFile}");

        builder.SetBasePath(pathToConfigFile) // for appsettings.json location
          .AddJsonFile(APP_SETTINGS_FILENAME, optional: false, reloadOnChange: false);

        configuration = configBuilder.Build();
      }
      catch (Exception ex)
      {
        log.LogCritical($"GenericConfiguration exception: {ex.Message}, {ex.Source}, {ex.StackTrace}");
        throw;
      }

      if (!configListed)
      {
        configListed = true;
        log.LogInformation("*************CONFIGURATION DETAILS*******************");
        //Log current configuration 
        foreach (var keyValuePair in configuration.AsEnumerable())
        {
          log.LogInformation($"{keyValuePair.Key}={keyValuePair.Value}");

        }
      }
    }

    private string PathToConfigFile()
    {
      if (log.IsTraceEnabled())
        log.LogTrace("Base:" + AppContext.BaseDirectory);
      var dirToAppsettings = Directory.GetCurrentDirectory();
      if (log.IsTraceEnabled())
        log.LogTrace("Current:" + dirToAppsettings);
      string pathToConfigFile;

      log.LogDebug(
        $"Testing default path for the config file {Directory.GetCurrentDirectory()} and {AppContext.BaseDirectory}");

      //Test if appsettings exists in the default folder for the console application
      if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), APP_SETTINGS_FILENAME)))
      {
        pathToConfigFile = Directory.GetCurrentDirectory();
      }
      else if (File.Exists(Path.Combine(AppContext.BaseDirectory, APP_SETTINGS_FILENAME)))
      {
        pathToConfigFile = AppContext.BaseDirectory;
      }
      else
      {
        var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
        pathToConfigFile = Path.GetDirectoryName(pathToExe);
        if (log.IsTraceEnabled())
          log.LogTrace($"No configuration files found, using alternative path {pathToConfigFile}");
      }

      return pathToConfigFile;
    }

    private void InitKubernetes()
    {
      lock (kubernetesInitLock)
      {
        var localConfig = new ConfigurationBuilder().AddEnvironmentVariables().SetBasePath(PathToConfigFile())
          .AddJsonFile(APP_SETTINGS_FILENAME, optional: false, reloadOnChange: false).Build();

        bool.TryParse(localConfig["UseKubernetes"], out var result);
        if (result && kubernetesInitialized == KubernetesState.NotInitialized)
        {
          kubernetesInitialized = KubernetesState.Requested;
          UseKubernetes = true;
          if (log.IsTraceEnabled())
            log.LogTrace("Setting variables for kubernetes");
          KubernetesConfigMapName = localConfig["KubernetesConfigMapName"];
          KubernetesNamespace = localConfig["KubernetesNamespace"];
          KubernetesContext = localConfig["KubernetesContext"];
        }

        if (!UseKubernetes)
        {
          log.LogInformation("Kubernetes plugin is not requested - exiting and disabling it permanently.");
          kubernetesInitialized = KubernetesState.NotRequired;
        }

        //try initialize Kubernetes
        if (kubernetesInitialized == KubernetesState.Requested)
        {
          try
          {
            if (log.IsTraceEnabled())
              log.LogTrace("Connecting to kubernetes cluster");
            KubernetesClientConfiguration config = null;
            if (string.IsNullOrWhiteSpace(KubernetesContext))
            {
              log.LogDebug("Using InCluster config");
              config = KubernetesClientConfiguration.InClusterConfig();
            }
            else
              config = KubernetesClientConfiguration.BuildConfigFromConfigFile(currentContext: KubernetesContext);


            var client = new Kubernetes(config);

            kubernetesConfig = new Dictionary<string, string>(client
              .ReadNamespacedConfigMapWithHttpMessagesAsync(KubernetesConfigMapName, KubernetesNamespace).Result.Body
              .Data);
            kubernetesInitialized = KubernetesState.Initialized;
            if (log.IsTraceEnabled())
              log.LogTrace("Successfully retrieved configuration from Kubernetes");
          }
          catch (Exception ex)
          {
            log.LogWarning(
              $"Can not connect to Kubernetes cluster with error {ex.Message}. Kubernetes is disabled for this process. at {ex.StackTrace}" );
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
          $"Your application is attempting to use the {connectionType} connectionType but is missing an environment variable. serverName: '{serverName}', serverPort: '{serverPort}', serverDatabaseName: '{serverDatabaseName}', serverUserName: '{serverUserName}', serverPassword: '{serverPassword}'";
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
      if (log.IsTraceEnabled())
        log.LogTrace($"Served connection string {connString}");

      return connString;
    }

    public string GetValueString(string key)
    {
      return GetValueString(key, null);
    }

    public string GetValueString(string key, string defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public int GetValueInt(string key)
    {
      return GetValueInt(key, int.MinValue);
    }

    public int GetValueInt(string key, int defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public uint GetValueUint(string key)
    {
      return GetValueUint(key, uint.MinValue);
    }

    public uint GetValueUint(string key, uint defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public long GetValueLong(string key)
    {
      return GetValueLong(key, long.MinValue);
    }

    public long GetValueLong(string key, long defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public double GetValueDouble(string key)
    {
      return GetValueDouble(key, double.MinValue);
    }

    public double GetValueDouble(string key, double defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public bool? GetValueBool(string key)
    {
      return GetValue< bool?> (key, (bool?)null);
    }

    public bool GetValueBool(string key, bool defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public TimeSpan? GetValueTimeSpan(string key)
    {
      return GetValue<TimeSpan?>(key, (TimeSpan?)null);
    }

    public TimeSpan GetValueTimeSpan(string key, TimeSpan defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public Guid GetValueGuid(string key)
    {
      return GetValue(key, Guid.Empty);
    }

    public Guid GetValueGuid(string key, Guid defaultValue)
    {
      return GetValue(key, defaultValue);
    }

    public IConfigurationSection GetSection(string key)
    {
      return configuration.GetSection(key);
    }

    public IConfigurationSection GetLoggingConfig()
    {
      return GetSection("Logging");
    }

    private T GetValue<T>(string key, T defaultValue)
    {
      T value = defaultValue;

      if (configuration[key] == null)
      {
        log.LogWarning($"Missing configuration key {key}");
      }
      else
      {
        try
        {
          value = configuration.GetValue(key, defaultValue);
        }
        catch (Exception e)
        {
          log.LogError($"Invalid configuration for key {key}: {e.Message}");
        }
      }   
      if (log.IsTraceEnabled())
        log.LogTrace($"Served configuration value {key}:{value}");
      return value;
    }
  }
}
