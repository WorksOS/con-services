#if DEBUG
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.ServiceDiscovery.Settings;

namespace VSS.Common.ServiceDiscovery.Resolvers
{
  public class DevelopmentServiceResolver : IServiceResolver
  {
    private readonly ILogger<DevelopmentServiceResolver> logger;

    private readonly DevelopmentSettings settings;

    public DevelopmentServiceResolver(ILogger<DevelopmentServiceResolver> logger)
    {
      this.logger = logger;
      Priority = 0; // Top priority

      settings = File.Exists(DevelopmentSettings.Filename) 
        ? JsonConvert.DeserializeObject<DevelopmentSettings>(File.ReadAllText(DevelopmentSettings.Filename)) 
        : null;
    }

    public Task<string> ResolveService(string serviceName)
    {
      string result = null;
      if (settings != null && settings.SelectedSettings.ContainsKey(serviceName))
      {
        result = settings.SelectedSettings[serviceName];
        logger.LogInformation($"Found value `{result}` for Service Name `{serviceName}`");
      }
      
      return Task.FromResult(result);
    }

    public ServiceResultType ServiceType => ServiceResultType.Development;

    public int Priority { get; private set; }
  }
}
#endif