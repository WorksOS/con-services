using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using DesignProfile = VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling.DesignProfile;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class DesignProfileDataRequestHelper : DataRequestBase, IProfileDesignRequestHandler
  {
    private IASNodeClient _raptorClient;

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
      _raptorClient = raptorClient;
      return this;
    }

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class and populate it with data needed for a Slicer profile.   
    /// </summary>
    /// <returns>An instance of the ProfileProductionDataRequest class.</returns>
    public ProfileResult CreateDesignProfileResponse(Guid projectUid, double latRadians1, double lngRadians1, double latRadians2, double lngRadians2, string designFilename, Guid importedFileUid, int importedFileTypeid, long alignmentId, Guid callId)
    {
      var designDescriptor = GetDescriptor(projectUid, importedFileUid, designFilename);

      var memoryStream = _raptorClient.GetDesignProfile(
        DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args(
          ProjectId,
          false,
          TWGS84Point.Point(lngRadians1, latRadians1),
          TWGS84Point.Point(lngRadians2, latRadians2),
          ValidationConstants.MIN_STATION,
          ValidationConstants.MAX_STATION,
          designDescriptor,
          RaptorConverters.EmptyDesignDescriptor,
          null,
          false));

      var profile = new DesignProfile
      {
        callId = callId,
        importedFileTypeID = importedFileTypeid,
        vertices = null,
        success = memoryStream != null
      };

      if (profile.success)
      {
        var pdsiProfile = new DesignProfile();

        memoryStream.Close();

      }


      return null;
    }

    private TVLPDDesignDescriptor GetDescriptor(Guid projectUid, Guid importedFileUid, string designFilename)
    {
      var fileList = FileListProxy.GetFiles(projectUid.ToString(), Headers).Result;

      if (fileList.Count <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Project has no appropriate design files."));
      }

      var designFile = fileList.SingleOrDefault(
        f => f.ImportedFileUid ==
             importedFileUid.ToString() &&
             f.IsActivated &&
             f.ImportedFileType == ImportedFileType.DesignSurface);

      if (designFile == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to access design file."));
      }

      var alignmentDescriptor = RaptorConverters.DesignDescriptor(
        designFile.LegacyFileId,
        designFile.ImportedFileUid,
        designFile.Path,
        designFilename,
        0);

      return alignmentDescriptor;
    }
  }
}