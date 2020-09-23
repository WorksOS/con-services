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
    private readonly ILogger<DevelopmentServiceResolver> _log;
    private readonly DevelopmentSettings _settings;

    public DevelopmentServiceResolver(ILogger<DevelopmentServiceResolver> logger)
    {
      _log = logger;
      Priority = 0; // Top priority ( if we have a file )

      try
      {
        _settings = File.Exists(DevelopmentSettings.Filename)
          ? JsonConvert.DeserializeObject<DevelopmentSettings>(File.ReadAllText(DevelopmentSettings.Filename))
          : null;
      }
      catch (JsonException e)
      {
        logger.LogError(e, $"Failed to process Settings File at {DevelopmentSettings.Filename}");
        _settings = null;
      }

      if (_settings == null)
      {
        Priority = int.MaxValue;
        logger.LogInformation("No settings file found, decreasing discovery priority.");
      }
    }

    public Task<string> ResolveService(string serviceName)
    {
      string result = null;

      if (_settings?.SelectedSettings.ContainsKey(serviceName) == true)
      {
        result = _settings.SelectedSettings[serviceName];
        _log.LogInformation($"Found value '{result}' for Service Name '{serviceName}'");
      }

      return Task.FromResult(result);
    }

    public ServiceResultType ServiceType => ServiceResultType.Development;

    public int Priority { get; }

    public bool IsEnabled => _settings != null;
  }
}
