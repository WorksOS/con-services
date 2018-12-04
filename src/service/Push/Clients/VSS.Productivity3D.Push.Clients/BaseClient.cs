using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
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
      Connection = new HubConnectionBuilder()
        .WithUrl(Configuration.GetValueString(UrlKey), options =>
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

        await Task.Factory.StartNew(TryConnect).ConfigureAwait(false);
      };

      SetupCallbacks();

      return Task.Factory.StartNew(TryConnect);
    }

    /// <summary>
    /// Actually does the connection, and keeps retrying until it connects
    /// </summary>
    private async Task TryConnect()
    {
      while (true)
      {
        try
        {
          await Connection.StartAsync();
          Connected = true;
          Logger.LogInformation($"Connected to `{Configuration.GetValueString(UrlKey)}`");
          break;
        }
        catch (HttpRequestException e)
        {
          Logger.LogError($"Failed to connect due to exception '{e.Message}' - Is the Server online?");
          await Task.Delay(RECONNECT_DELAY_MS);
        }
      }
    }
  }
}