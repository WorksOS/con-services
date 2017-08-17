using Microsoft.Extensions.Logging;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Extensions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class DesignProfileDataRequestHelper : DataRequestBase, IProfileDesignRequestHandler
  {
    public DesignProfileDataRequestHelper()
    { }

    public DesignProfileDataRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<SliceProfileDataRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }
    public DesignProfileDataRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      return this;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a Slicer profile.   
    /// </summary>
    /// <returns>An instance of the ProfileProductionDataRequest class.</returns>
    public ProfileProductionDataRequest CreateDesignProfileResponse(Guid projectUid, double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees, Guid customerUid, Guid importedFileUid, Guid filterUid)
    {
      var llPoints = ProfileLLPoints.CreateProfileLLPoints(startLatDegrees.latDegreesToRadians(), startLonDegrees.lonDegreesToRadians(), endLatDegrees.latDegreesToRadians(), endLonDegrees.lonDegreesToRadians());

      var filter = SettingsManager.CompactionFilter(filterUid.ToString(), projectUid.ToString(), Headers);
      var designDescriptor = GetDescriptor(projectUid, importedFileUid);
      var liftBuildSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      // callId is set to 'empty' because raptor will create and return a Guid if this is set to empty.
      // this would result in the acceptance tests failing to see the callID == in its equality test
      return ProfileProductionDataRequest.CreateProfileProductionData(
        ProjectId,
        Guid.Empty,
        ProductionDataType.Height,
        filter,
        -1,
        designDescriptor,
        null,
        llPoints,
        ValidationConstants.MIN_STATION,
        ValidationConstants.MIN_STATION,
        liftBuildSettings,
        false);
    }
  }
}