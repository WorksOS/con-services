using Microsoft.Extensions.Logging;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class DesignProfileDataRequestHelper: DataRequestBase, IProfileDesignRequestHandler
  {
    public DesignProfileDataRequestHelper()
    { }

    public DesignProfileDataRequestHelper(ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      log = logger.CreateLogger<SliceProfileDataRequestHelper>();
      this.configStore = configStore;
      this.fileListProxy = fileListProxy;
      this.settingsManager = settingsManager;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a Slicer profile.   
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="startLatDegrees"></param>
    /// <param name="startLonDegrees"></param>
    /// <param name="endLatDegrees"></param>
    /// <param name="endLonDegrees"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <param name="cutfillDesignUid"></param>
    /// <returns>An instance of the ProfileProductionDataRequest class.</returns>
    public ProfileProductionDataRequest CreateDesignProfileResponse()
    {
      throw new NotImplementedException();
    }

    public ProfileProductionDataRequest CreateSlicerProfileResponse(Guid projectUid, double startLatDegrees,
      double startLonDegrees, double endLatDegrees, double endLonDegrees, DateTime? startUtc, DateTime? endUtc,
      Guid? cutfillDesignUid)
    {
      throw new NotImplementedException();
    }
  }
}