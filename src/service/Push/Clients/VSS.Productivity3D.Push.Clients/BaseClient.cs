using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Push.Abstractions;

namespace VSS.Productivity3D.Push.Clients
{
  public abstract class BaseClient : IHubClient
  {
    /// <summary>
    /// Time to wait between connection attempts
    /// </summary>
    private const int RECONNECT_DELAY_MS = 1000;

    protected readonly IConfigurationStore Configuration;
    
    protected ILogger Logger;
    protected HubConnection Connection;
    
    protected BaseClient(IConfigurationStore configuration, ILoggerFactory loggerFactory)
    {
      Logger = loggerFactory.CreateLogger(GetType().Name);
      Configuration = configuration;
    }

    /// <inheritdoc />
    public bool Connected { get; private set; }

    /// <summary>
    /// The URL Key for the Configuration Store
    /// </summary>
    public abstract string UrlKey { get; }

    /// <summary>
    /// Method to setup any callbacks from the SignalR Hub
    /// </summary>
    public abstract void SetupCallbacks();

    /// <inheritdoc />
    public Task Disconnect()
    {
      Connected = false;
      return Connection != null 
        ? Connection.DisposeAsync() 
        : Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task Connect()
    {
      if (string.IsNullOrWhiteSpace(UrlKey))
      {
        // This should be set in code
        Logger.LogCritical($"No URL Key provided to Push Client - not starting");
        return Task.CompletedTask;
      }

      var url = Configuration.GetValueString(UrlKey, string.Empty);
      if (string.IsNullOrEmpty(url))
      {
        Logger.LogWarning($"Cannot find key {UrlKey} in settings, not connecting....");
        return Task.CompletedTask;
      }

      Connection = new HubConnectionBuilder()
        .WithUrl(url, options =>
        {
          if (Configuration.GetValueBool("PUSH_NO_AUTHENTICATION_HEADER", false))
          {
            Logger.LogInformation("Attempting to skip TPaaS Authentication");
            options.Headers.Add("X-VSS-NO-TPAAS", "true");
          }
          else
          {
            Logger.LogWarning("No authentication headers added.");
          }

        }).Build();

      Connection.Closed += async (e) =>
      {
        Logger.LogError(e, $"Lost Connection to `{Configuration.GetValueString(UrlKey)}`");
        Connected = false;
        await Task.Factory.StartNew(TryConnect,TaskCreationOptions.LongRunning).ConfigureAwait(false);
      };

      SetupCallbacks();

      return Task.Factory.StartNew(TryConnect, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Actually does the connection, and keeps retrying until it connects
    /// </summary>
    private async Task TryConnect()
    {
      var tokenSource = new CancellationTokenSource(RECONNECT_DELAY_MS);
      while (true)
      {
        try
        {
          Logger.LogDebug($"Connecting to `{Configuration.GetValueString(UrlKey)}`");
          await Connection.StartAsync(tokenSource.Token);
          Connected = true;
          Logger.LogInformation($"Connected to `{Configuration.GetValueString(UrlKey)}`");
          break;
        }
        catch (HttpRequestException e)
        {
          Logger.LogError(e, "Failed to connect due to exception - Is the Server online?");
          await Task.Delay(RECONNECT_DELAY_MS);
        }
      }
    }
  }
}
