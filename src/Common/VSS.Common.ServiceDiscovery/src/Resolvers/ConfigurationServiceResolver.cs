using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Models;

namespace VSS.Common.ServiceDiscovery.Resolvers
{
  /// <summary>
  /// Uses the IConfiguration provided to find Services
  /// Note depending on Startup, this could include environment variables and/or appsettings.json
  /// </summary>
  public class ConfigurationServiceResolver : IServiceResolver
  {
    private const int DEFAULT_PRIORITY = 10;

    private readonly ILogger<ConfigurationServiceResolver> logger;
    private readonly IConfiguration configuration;

    public ConfigurationServiceResolver(ILogger<ConfigurationServiceResolver> logger, IConfiguration configuration)
    {
      this.logger = logger;
      this.configuration = configuration;
      if (int.TryParse(configuration["ConfigurationServicePriority"], out var p))
        Priority = p;
      else
      {
        logger.LogWarning($"Cannot find priority, defaulting to {DEFAULT_PRIORITY}");
        Priority = DEFAULT_PRIORITY;
      }
    }

    public Task<string> ResolveService(string serviceName)
    {
      var configValue = configuration[serviceName];

      if (!string.IsNullOrEmpty(configValue))
      {
        logger.LogDebug($"Service `{serviceName}` was found in configuration");
        return Task.FromResult(configValue);
      }

      logger.LogWarning($"Could not find any service with name `{serviceName}` in Configuration");

      return Task.FromResult<string>(null);
    }

    public ServiceResultType ServiceType => ServiceResultType.Configuration;
    public int Priority { get; }
  }
}