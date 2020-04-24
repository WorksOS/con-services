using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.ConfigurationStore;

namespace VSS.Common.ServiceDiscovery.Resolvers
{
  /// <summary>
  /// Uses the IConfiguration provided to find Services
  /// Note depending on Startup, this could include environment variables and/or appsettings.json
  /// </summary>
  public class ConfigurationServiceResolver : IServiceResolver
  {
    private const int DEFAULT_PRIORITY = 100;

    private readonly ILogger<ConfigurationServiceResolver> logger;
    private readonly IConfigurationStore configuration;

    public ConfigurationServiceResolver(ILogger<ConfigurationServiceResolver> logger, IConfigurationStore configuration)
    {
      this.logger = logger;
      this.configuration = configuration;
      Priority = configuration.GetValueInt("ConfigurationServicePriority", DEFAULT_PRIORITY);
    }

    public Task<string> ResolveService(string serviceName)
    {
      var configValue = configuration.GetValueString(serviceName, null);
      configValue = configValue ?? configuration.GetValueString(serviceName.Replace('-','_'), null);

      if (!string.IsNullOrEmpty(configValue))
      {
        logger.LogDebug($"Service `{serviceName}` was found in configuration");
        return Task.FromResult(configValue);
      }

      return Task.FromResult<string>(null);
    }

    public ServiceResultType ServiceType => ServiceResultType.Configuration;
    public int Priority { get; }

    public bool IsEnabled => true;
  }
}
