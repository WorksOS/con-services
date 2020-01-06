using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Productivity3D.Push.Abstractions;

namespace VSS.Productivity3D.Push.Clients
{
  public abstract class BaseClient : IHubClient
  {
    /// <summary>
    /// Time to wait between connection attempts
    /// </summary>
    private const int RECONNECT_DELAY_MS = 1000;

    public const string PUSH_REQUEST_NO_AUTH = "PUSH_NO_AUTHENTICATION_HEADER";

    public const string SKIP_AUTHENTICATION_HEADER = "X-VSS-NO-TPAAS";

    protected readonly IConfigurationStore Configuration;
    
    protected ILogger Logger;
    protected HubConnection Connection;

    private readonly IServiceResolution serviceDiscovery;

    private Uri endpoint;

    protected BaseClient(IConfigurationStore configuration, IServiceResolution serviceDiscovery, ILoggerFactory loggerFactory)
    {
      Logger = loggerFactory.CreateLogger(GetType().Name);
      Configuration = configuration;
      this.serviceDiscovery = serviceDiscovery;
    }

    private bool _connected;

    /// <inheritdoc />
    public bool Connected
    {
      get => _connected;
      private set
      {
        IsConnecting = false;
        _connected = value;
        SignalRHealthCheck.State = value;
      }
    }
    
    /// <inheritdoc />
    public bool IsConnecting { get; private set; }

    /// <summary>
    /// The Route for the hub, which is appended to the Push URL
    /// </summary>
    public abstract string HubRoute { get; }

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
    public async Task Connect()
    {
      if (string.IsNullOrWhiteSpace(HubRoute))
      {
        throw new ArgumentException("No URL Key provided to Push Client - not starting", nameof(HubRoute));
      }

      await Task.Factory.StartNew(TryConnect);
    }
    
    /// <summary>
    /// Actually does the connection, and keeps retrying until it connects
    /// </summary>
    private async Task TryConnect()
    {
      Logger.LogInformation($"{nameof(TryConnect)} HubRoute: {HubRoute}");
      if (Connected || IsConnecting)
        return;

      IsConnecting = true;
      while (true)
      {
        try
        {
          // If the URL of the endpoint changes, we need to be able to connect to the new URL
          // SignalR doesn't give us a way to set a new url without recreating the Connection Object
          await SetupConnection();
          if (Connection == null)
          {
            Connected = false;
            await Task.Delay(RECONNECT_DELAY_MS); 
          }
          else
          {
            await Connection.StartAsync();
            Connected = true;
            Logger.LogInformation($"Connected to `{endpoint.AbsolutePath}`");
          }

          break;
        }
        catch (HttpRequestException e)
        {
          Connected = false;
          // This is a known error, if there is an connection closed (due to pod restarting, or network issue)
          Logger.LogError(e, "Failed to connect due to exception - Is the Server online?");
          await Task.Delay(RECONNECT_DELAY_MS);
        }
        catch (Exception e)
        {
          Connected = false;
          // We need to catch all exceptions, if we don't the reconnection thread will be stopped.
          Logger.LogError(e, "Failed to connect due to exception - Unknown exception occured.");
          await Task.Delay(RECONNECT_DELAY_MS);
        }
        
      }
    }

    /// <summary>
    /// Setup a connection to Push Service, using service resolution
    /// </summary>
    private async Task SetupConnection()
    {
      if (Connection != null)
      {
        await Disconnect();
        Connection = null;
      }

      var serviceResult = await serviceDiscovery.ResolveService(ServiceNameConstants.PUSH_SERVICE);
      if (serviceResult.Type == ServiceResultType.Unknown || string.IsNullOrEmpty(serviceResult.Endpoint))
      {
        Logger.LogWarning($"Cannot find the service `{ServiceNameConstants.PUSH_SERVICE}`.");
        return;
      }

      endpoint = new Uri(new Uri(serviceResult.Endpoint), HubRoute);

      Logger.LogInformation($"{nameof(SetupConnection)} HubRoute: {HubRoute} endpoint: {endpoint}");
      Connection = new HubConnectionBuilder()
        .WithUrl(endpoint, options =>
        {
          if (Configuration.GetValueBool(PUSH_REQUEST_NO_AUTH, false))
          {
            Logger.LogInformation("Attempting to skip TPaaS Authentication");
            options.Headers.Add(SKIP_AUTHENTICATION_HEADER, "true");
          }
          else
          {
            Logger.LogWarning("No authentication headers added.");
          }
        }).Build();

      Connection.Closed += async (e) =>
      {
        Logger.LogError(e, $"Lost Connection to `{endpoint.AbsolutePath}`");
        Connected = false;

        await Task.Factory.StartNew(TryConnect).ConfigureAwait(false);
      };

      // We must call setup callbacks after we setup the connection
      SetupCallbacks();
    }
  }
}
