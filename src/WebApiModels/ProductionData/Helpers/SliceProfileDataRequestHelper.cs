using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Extensions;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class SliceProfileDataRequestHelper : DataRequestBase, IProfileSliceRequestHandler
  {
    public SliceProfileDataRequestHelper()
    { }

    public SliceProfileDataRequestHelper(ILoggerFactory logger, IConfigurationStore configStore,
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
    public ProfileProductionDataRequest CreateSlicerProfileResponse(Guid projectUid,
      double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees,
      DateTime? startUtc, DateTime? endUtc, Guid? cutfillDesignUid)
    {
      var llPoints = ProfileLLPoints.CreateProfileLLPoints(startLatDegrees.latDegreesToRadians(), startLonDegrees.lonDegreesToRadians(), endLatDegrees.latDegreesToRadians(), endLonDegrees.lonDegreesToRadians());

      var filter = settingsManager.CompactionFilter(startUtc, endUtc, null, null, null, null, null, ExcludedIds);

      DesignDescriptor designDescriptor = null;
      if (cutfillDesignUid.HasValue)
      {

        var fileList = fileListProxy.GetFiles(projectUid.ToString(), Headers).Result;

        if (fileList.Count > 0)
        {
          var designFile = fileList.SingleOrDefault(f => f.ImportedFileUid == cutfillDesignUid.Value.ToString() &&
                                                f.IsActivated &&
                                                f.ImportedFileType == ImportedFileType.DesignSurface);

          if (designFile != null)
          {
            designDescriptor = DesignDescriptor.CreateDesignDescriptor(designFile.LegacyFileId, FileDescriptor.CreateFileDescriptor(GetFilespaceId(), designFile.Path, designFile.Name), 0);
          }
          else
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                "Unable to access design file."));
          }
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Project has no appropriate design files."));
        }
      }

      var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(ProjectSettings);

      // callId is set to 'empty' because raptor will create and return a Guid if this is set to empty.
      // this would result in the acceptance tests failing to see the callID == in its equality test
      return ProfileProductionDataRequest.CreateProfileProductionData(ProjectId, Guid.Empty, ProductionDataType.Height, filter, -1,
        designDescriptor, null, llPoints, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, liftBuildSettings, false);
    }

    /// <summary>
    /// Gets the TCC filespaceId for the vldatastore filespace
    /// </summary>
    private string GetFilespaceId()
    {
      var filespaceId = configStore.GetValueString("TCCFILESPACEID");
      if (!string.IsNullOrEmpty(filespaceId))
      {
        return filespaceId;
      }

      const string errorString = "Your application is missing an environment variable TCCFILESPACEID";
      log.LogError(errorString);
      throw new InvalidOperationException(errorString);
    }
  }
}