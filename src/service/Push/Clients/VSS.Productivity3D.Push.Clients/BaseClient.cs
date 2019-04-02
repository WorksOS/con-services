using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
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
    private IServiceResolution ServiceDiscovery;


    protected BaseClient(IConfigurationStore configuration, IServiceResolution serviceDiscovery, ILoggerFactory loggerFactory)
    {
      Logger = loggerFactory.CreateLogger(GetType().Name);
      Configuration = configuration;
      ServiceDiscovery = serviceDiscovery;
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
    public async Task Connect()
    {
      if (string.IsNullOrWhiteSpace(UrlKey))
      {
        // This should be set in code
        Logger.LogCritical($"No URL Key provided to Push Client - not starting");
        return;
      }

      var url = await ServiceDiscovery.ResolveService(ServiceNameConstants.PUSH_SERVICE);
      if (url.Type == ServiceResultType.Unknown || string.IsNullOrEmpty(url.Endpoint))
      {
        Logger.LogWarning($"Cannot find key {UrlKey} in settings, not connecting....");
        return;
      }

      Connection = new HubConnectionBuilder()
        .WithUrl(new Uri($"{url.Endpoint}{UrlKey}"), options =>
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

      await Task.Factory.StartNew(TryConnect);
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
          // This is a known error, if there is an connection closed (due to pod restarting, or network issue)
          Logger.LogError($"Failed to connect due to exception - Is the Server online? Message: {e.Message}");
          await Task.Delay(RECONNECT_DELAY_MS);
        }
        catch (Exception e)
        {
          // We need to catch all exceptions, if we don't the reconnection thread will be stopped.
          Logger.LogError(e, "Failed to connect due to exception - Unknown exception occured.");
          await Task.Delay(RECONNECT_DELAY_MS);
        }
      }
    }
  }
}
