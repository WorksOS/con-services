using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Helper to create a cut-fill details request.
  /// </summary>
  public class CutFillRequestHelper : DataRequestBase, ICutFillRequestHelper
  {
    public CutFillRequestHelper()
    { }

    public CutFillRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    /// <summary>
    /// Creates an instance of the CutFillDetailsRequest class and populate it with data needed for a cut-fill details request.   
    /// </summary>
    /// <returns>An instance of the CutFillDetailsRequest class.</returns>
    public CutFillDetailsRequest CreateCutFillDetailsRequest()
    {
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);
      var cutFillSettings = SettingsManager.CompactionCutFillSettings(ProjectSettings);
      return CutFillDetailsRequest.CreateCutFillDetailsRequest(ProjectId, cutFillSettings, Filter, liftSettings, DesignDescriptor);
    }
  }
}
