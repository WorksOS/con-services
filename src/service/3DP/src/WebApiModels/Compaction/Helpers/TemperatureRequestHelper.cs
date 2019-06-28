using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Helper to create a temperature details request.
  /// </summary>
  public class TemperatureRequestHelper : DataRequestBase
  {
    public TemperatureRequestHelper()
    { }

    public TemperatureRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileImportProxy = fileImportProxy;
      SettingsManager = settingsManager;
    }

    /// <summary>
    /// Creates an instance of the TemperatureDetailsRequest class and populate it with data needed for a temperature details request.   
    /// </summary>
    /// <returns>An instance of the TemperatureDetailsRequest class.</returns>
    public TemperatureDetailsRequest CreateTemperatureDetailsRequest()
    {
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);
      var temperatureSettings = SettingsManager.CompactionTemperatureDetailsSettings(ProjectSettings);
      return new TemperatureDetailsRequest(ProjectId, ProjectUid, temperatureSettings, Filter, liftSettings);
    }
  }
}
