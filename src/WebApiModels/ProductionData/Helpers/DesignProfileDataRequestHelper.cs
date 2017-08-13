using Microsoft.Extensions.Logging;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class DesignProfileDataRequestHelper : DataRequestBase, IProfileDesignRequestHandler
  {
    public DesignProfileDataRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<SliceProfileDataRequestHelper>();
      this.ConfigurationStore = configurationStore;
      this.FileListProxy = fileListProxy;
      this.SettingsManager = settingsManager;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a Slicer profile.   
    /// </summary>
    /// <returns>An instance of the ProfileProductionDataRequest class.</returns>
    public ProfileProductionDataRequest CreateDesignProfileResponse()
    {
      throw new NotImplementedException();
    }
  }
}