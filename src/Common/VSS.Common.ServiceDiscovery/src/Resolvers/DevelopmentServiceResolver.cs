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
      Priority = 0; // Top priority ( if we have a file )

      try
      {
        settings = File.Exists(DevelopmentSettings.Filename)
          ? JsonConvert.DeserializeObject<DevelopmentSettings>(File.ReadAllText(DevelopmentSettings.Filename))
          : null;
      }
      catch (JsonException e)
      {
        logger.LogError(e, $"Failed to process Settings File at {DevelopmentSettings.Filename}");
        settings = null;
      }

      if (settings == null)
      {
        Priority = int.MaxValue;
        logger.LogInformation("No settings file found, decreasing discovery priority.");
      }
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

    public bool IsEnabled => settings != null;
  }
}
