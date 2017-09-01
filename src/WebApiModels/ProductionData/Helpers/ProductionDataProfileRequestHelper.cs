using Microsoft.Extensions.Logging;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Extensions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ProductionDataProfileRequestHelper : DataRequestBase, IProductionDataProfileRequestHelper
  {
    public ProductionDataProfileRequestHelper()
    { }

    public ProductionDataProfileRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    /// <summary>
    /// Creates an instance of the CompactionProfileProductionDataRequest class and populate it with data needed for a production data slice profile.   
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="startLatDegrees"></param>
    /// <param name="startLonDegrees"></param>
    /// <param name="endLatDegrees"></param>
    /// <param name="endLonDegrees"></param>
    /// <param name="customerUid"></param>
    /// <param name="cutfillDesignUid"></param>
    /// <param name="filterUid"></param>
    /// <returns>An instance of the CompactionProfileProductionDataRequest class.</returns>
    public CompactionProfileProductionDataRequest CreateProductionDataProfileRequest(Guid projectUid,
      double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees, Guid customerUid, Guid? cutfillDesignUid)
    {
      var llPoints = ProfileLLPoints.CreateProfileLLPoints(startLatDegrees.latDegreesToRadians(), startLonDegrees.lonDegreesToRadians(), endLatDegrees.latDegreesToRadians(), endLonDegrees.lonDegreesToRadians());

      DesignDescriptor designDescriptor = null;
      if (cutfillDesignUid.HasValue)
      {
        designDescriptor = GetDescriptor(projectUid, cutfillDesignUid.Value);
      }

      var liftBuildSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      // callId is set to 'empty' because raptor will create and return a Guid if this is set to empty.
      // this would result in the acceptance tests failing to see the callID == in its equality test
      return CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(
        ProjectId,
        Guid.Empty,
        ProductionDataType.Height,
        Filter,
        -1,
        null,
        null,
        llPoints,
        ValidationConstants.MIN_STATION,
        ValidationConstants.MIN_STATION,
        liftBuildSettings,
        false,
        designDescriptor);
    }
  }
}