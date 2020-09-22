using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CCSS.WorksOS.Healthz.Models;
using CCSS.WorksOS.Healthz.Responses;
using CCSS.WorksOS.Healthz.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.WorksOS.Healthz.Services
{
  public class HealthCheckService : BaseHostedService, IHealthCheckService
  {
    private readonly IWebRequest _webRequest;
    private readonly IHealthCheckState _healthCheckState;
    private readonly IServiceResolution _serviceResolution;

    private bool IsSuccessStatusCode(HttpStatusCode statusCode) => (int)statusCode >= 200 && (int)statusCode <= 299;

    public HealthCheckService(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScope, IServiceResolution serviceResolution, IWebRequest webRequest, IHealthCheckState healthCheckState)
       : base(loggerFactory, serviceScope)
    {
      _serviceResolution = serviceResolution;
      _webRequest = webRequest;
      _healthCheckState = healthCheckState;
    }

    /// <inheritdoc/>
    public IEnumerable<Service> GetServiceIdentifiers() => _healthCheckState.GetServiceIdentifiers();

    /// <inheritdoc/>
    public IEnumerable<ServicePingResponse> GetServiceState(string[] serviceIdentifiers) => _healthCheckState.GetServiceState(serviceIdentifiers);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      var serviceIdentifiers = ServiceResolver.GetKnownServiceIdentifiers();

      if (serviceIdentifiers.Count == 0)
      {
        Logger.LogError($"{nameof(ExecuteAsync)} No service identifiers found, exiting.");
        return;
      }

      await ResolvePollingServices(serviceIdentifiers);

      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          await ResolveServicesState(_healthCheckState.GetServiceIdentifiers().ToList());

          await Task.Delay(10000, cancellationToken);
        }
        catch (Exception e)
        {
          Logger.LogError(e, $"{nameof(ExecuteAsync)} Failed to resolve polling services");

          await Task.Delay(60 * 1000, cancellationToken);
        }
      }
    }

    /// <summary>
    /// Get and cache all known and available services using service discovery.
    /// </summary>
    private async Task ResolvePollingServices(List<string> serviceIdentifiers)
    {
      var serviceResultTasks = new List<Task>(serviceIdentifiers.Count);
      var _services = new Dictionary<string, ServiceResult>();

      Logger.LogInformation($"{nameof(ResolvePollingServices)}: Resolving services for polling...");

      foreach (var identifier in serviceIdentifiers)
      {
        Logger.LogInformation($"{nameof(ResolvePollingServices)}: Testing service '{identifier}'...");

        serviceResultTasks
          .Add(_serviceResolution.ResolveService(serviceName: identifier)
          .ContinueWith(x =>
          {
            var serviceResult = x.Result;

            if (string.IsNullOrEmpty(serviceResult.Endpoint) ||
                serviceResult.Endpoint.Contains("localhost") ||
                serviceResult.Endpoint.Contains("healthz"))
            {
              Logger.LogDebug($"{nameof(ResolvePollingServices)}: Filtering out service '{identifier}' ({serviceResult.Endpoint}) from the polling services list.");

              return;
            }

            if (!_services.TryGetValue(identifier, out var _))
            {
              Logger.LogInformation($"{nameof(ResolvePollingServices)}: Found service '{identifier}' listening on '{serviceResult.Endpoint}'");

              _healthCheckState.AddPollingService(new Service(identifier, serviceResult.Endpoint));
            }
          }));
      }

      await Task.WhenAll(serviceResultTasks);
    }

    /// <summary>
    /// Poll each service identifer using serivce discovery to determine their state of responsiveness.
    /// </summary>
    private async Task ResolveServicesState(List<Service> serviceIdentifiers)
    {
      var serviceStateTasks = new List<Task>(serviceIdentifiers.Count);
      var _services = new Dictionary<string, ServiceResult>();

      Logger.LogInformation($"{nameof(ResolvePollingServices)}: Resolving services for polling...");

      var aggregatedServiceState = ServiceState.Unknown;

      foreach (var service in serviceIdentifiers)
      {
        serviceStateTasks
          .Add(QueryService(service)
          .ContinueWith(x =>
          {
            var servicePingResponse = x.Result;

            if (!x.IsCompleted)
            {
              Logger.LogError($"{nameof(ResolveServicesState)}: Failure querying service '{service.Identifier}' at '{service.Endpoint}'; {x.Exception.GetBaseException().Message}");
              return;
            }

            _healthCheckState.AddServicePingResponse(service.Identifier, servicePingResponse);

            if (servicePingResponse.State != ServiceState.Available)
            {
              aggregatedServiceState = ServiceState.Unavailable;
            }

            if (aggregatedServiceState != ServiceState.Unavailable)
            {
              aggregatedServiceState = ServiceState.Available;
            }
          }));
      }

      await Task.WhenAll(serviceStateTasks);

      Logger.LogInformation($"{nameof(ResolveServicesState)}: Setting aggregated service state to: {aggregatedServiceState}");
      _healthCheckState.SetAggregatedServiceState(aggregatedServiceState);
    }

    /// <summary>
    /// Query a service on it's predefined 'ping' endpoint.
    /// </summary>
    private async Task<ServicePingResponse> QueryService(Service service)
    {
      var pingUrl = service.Endpoint.TrimEnd('/') + "/ping";

      try
      {
        Logger.LogInformation($"{nameof(QueryService)}: Querying '{pingUrl}'...");
        var sw = Stopwatch.StartNew();

        var response = await _webRequest.ExecuteRequest(
          endpoint: pingUrl,
          method: HttpMethod.Get,
          retries: 0);

        sw.Stop();

        Logger.LogInformation($"{nameof(QueryService)}: Service '{service.Identifier}' responded in {sw.Elapsed}");
        return ServicePingResponse.Create(service.Identifier, sw.ElapsedTicks, IsSuccessStatusCode(response));
      }
      catch (SocketException ex)
      {
        Logger.LogWarning(ErrorMessage(ex));
        return null;
      }
      catch (Exception ex)
      {
        Logger.LogError(ex, ErrorMessage(ex));
        return null;
      }

      string ErrorMessage(Exception ex) => $"{nameof(QueryService)}: Failure querying service '{service.Identifier}' at '{pingUrl}'; {ex.GetBaseException().Message}";
    }
  }
}
