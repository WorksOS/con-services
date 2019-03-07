using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace VSS.Common.Abstractions.ServiceDiscovery
{
  public class InternalServiceResolver : IServiceResolution
  {
    private readonly ILogger<InternalServiceResolver> logger;

    public InternalServiceResolver(IEnumerable<IServiceResolver> serviceResolvers, ILogger<InternalServiceResolver> logger)
    {
      this.logger = logger;
      Resolvers = serviceResolvers.OrderBy(s => s.Priority).ToList();
      logger.LogInformation($"We have {Resolvers.Count} Service Resolvers:");
      logger.LogInformation("-----");
      foreach (var serviceResolver in Resolvers)
      {
        logger.LogInformation($"\t{serviceResolver.GetType().Name}");
        logger.LogInformation($"\t\tPriority: {serviceResolver.Priority}");
        logger.LogInformation($"\t\tService Type: {serviceResolver.ServiceType}");
      }
      logger.LogInformation("-----");
    }

    /// <inheritdoc />
    public List<IServiceResolver> Resolvers { get; }

    /// <inheritdoc />
    public async Task<ServiceResult> ResolveService(string serviceName)
    {
      foreach (var serviceResolver in Resolvers)
      {
        var endPoint = await serviceResolver.ResolveService(serviceName);
        if (!string.IsNullOrEmpty(endPoint))
        {
          return new ServiceResult
          {
            Endpoint = endPoint,
            Type = serviceResolver.ServiceType
          };
        }
      }
      logger.LogError($"Failed to find requested service {serviceName}");
      return null;
    }
  }
}