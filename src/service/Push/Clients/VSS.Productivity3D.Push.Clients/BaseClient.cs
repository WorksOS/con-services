using System;
using System.Net.Http;
using System.Threading;
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
    private CancellationTokenSource _cancellationTokenSource;

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

    /// <summary>
    /// Called when the connection is successfully created
    /// </summary>
    protected virtual void OnConnection()
    {
    }

    /// <inheritdoc />
    public Task Disconnect()
    {
      _cancellationTokenSource?.Cancel();
      Connected = false;
      return Connection != null 
        ? Connection.DisposeAsync() 
        : Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task Connect()
    {
      _cancellationTokenSource?.Cancel();
      _cancellationTokenSource = new CancellationTokenSource();
      if (string.IsNullOrWhiteSpace(HubRoute))
      {
        throw new ArgumentException("No URL Key provided to Push Client - not starting", nameof(HubRoute));
      }

      // We must start the connection in a background task, and not just await the 'TryConnect' Method
      // If we just awaited here, then a failure to connect would prevent this from continuing
      await Task.Factory.StartNew(() => TryConnect(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Actually does the connection, and keeps retrying until it connects
    /// </summary>
    private async Task TryConnect(CancellationToken token)
    {
      if (Connected || IsConnecting)
        return;

      IsConnecting = true;
      while (!token.IsCancellationRequested)
      {
        try
        {
          // If the URL of the endpoint changes, we need to be able to connect to the new URL
          // SignalR doesn't give us a way to set a new url without recreating the Connection Object
          await SetupConnection(token);
          if (Connection == null)
          {
            Connected = false;
            await Task.Delay(RECONNECT_DELAY_MS, token);
          }
          else
          {
            await Connection.StartAsync(token);
            Connected = true;
            Logger.LogInformation($"Connected to `{endpoint.AbsolutePath}`");
            OnConnection();
          }

          break;
        }
        catch (TaskCanceledException)
        {
          Connected = false;
          Logger.LogInformation("Connection has been cancelled. Service shutting down.");
        }
        catch (HttpRequestException e)
        {
          Connected = false;
          // This is a known error, if there is an connection closed (due to pod restarting, or network issue)
          Logger.LogError(e, "Failed to connect due to exception - Is the Server online?");
          await Task.Delay(RECONNECT_DELAY_MS, token);
        }
        catch (Exception e)
        {
          Connected = false;
          // We need to catch all exceptions, if we don't the reconnection thread will be stopped.
          Logger.LogError(e, "Failed to connect due to exception - Unknown exception occured.");
          await Task.Delay(RECONNECT_DELAY_MS, token);
        }
      }
    }

    /// <summary>
    /// Setup a connection to Push Service, using service resolution
    /// </summary>
    private async Task SetupConnection(CancellationToken token)
    {
      if (Connection != null)
      {
        await Connection.DisposeAsync();
        Connection = null;
      }

      // If the service is being deployed, the kubernetes service may not have been created (as the previous release sometimes gets deleted).
      // We want to wait for this to come back up.
      // We must have a connection to the push service for any form of caching to be used correctly.
      var serviceResult = await serviceDiscovery.ResolveService(ServiceNameConstants.PUSH_SERVICE);
      while(serviceResult.Type == ServiceResultType.Unknown || string.IsNullOrEmpty(serviceResult.Endpoint))
      {
        if (token.IsCancellationRequested)
          return;

        Logger.LogWarning($"Cannot find the service `{ServiceNameConstants.PUSH_SERVICE}` - maybe it is starting up. Waiting...");
        await Task.Delay(1000, token);
        serviceResult = await serviceDiscovery.ResolveService(ServiceNameConstants.PUSH_SERVICE);
      }

      Logger.LogInformation($"Found URL of `{serviceResult.Endpoint}` via Service Discovery Type `{serviceResult.Type}` for {ServiceNameConstants.PUSH_SERVICE}");

      endpoint = new Uri(new Uri(serviceResult.Endpoint), HubRoute);

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

        await Task.Run(() => TryConnect(token), token);
      };

      // We must call setup callbacks after we setup the connection
      SetupCallbacks();
    }
  }
}
