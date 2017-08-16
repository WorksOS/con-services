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
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
    public ProfileProductionDataRequest CreateDesignProfileResponse(Guid projectUid, double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees, Guid filterUid, Guid customerUid, Guid importedFileUid, int importedFileTypeid, long alignmentId)
    {
      var llPoints = ProfileLLPoints.CreateProfileLLPoints(startLatDegrees.latDegreesToRadians(), startLonDegrees.lonDegreesToRadians(), endLatDegrees.latDegreesToRadians(), endLonDegrees.lonDegreesToRadians());

      var filter = SettingsManager.CompactionFilter(filterUid.ToString(), customerUid.ToString(), projectUid.ToString(),
        Headers);

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
        true);


      //ProfilesHelper.convertProfileEndPositions(null, llPoints, out TWGS84Point startPt, out TWGS84Point endPt, out bool positionsAreGrid);


      //var designProfile = DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args(
      //  ProjectId,
      //  false,
      //  //TWGS84Point.Point(startLatDegrees.latDegreesToRadians(), startLonDegrees.lonDegreesToRadians()),
      //  //TWGS84Point.Point(endLatDegrees.latDegreesToRadians(), endLonDegrees.lonDegreesToRadians()),
      //  startPt,
      //  endPt,
      //  ValidationConstants.MIN_STATION,
      //  ValidationConstants.MAX_STATION,
      //  designDescriptor,
      //  RaptorConverters.EmptyDesignDescriptor,
      //  null,
      //  false);


      //var memoryStream = _raptorClient.GetDesignProfile(designProfile);

      //return null;

      //var profile = new DesignProfile
      //{
      //  callId = callId,
      //  importedFileTypeID = importedFileTypeid,
      //  vertices = null,
      //  success = memoryStream != null
      //};

      //if (profile.success)
      //{
      //  var pdsiProfile = new DesignProfile();

      //  memoryStream.Close();


      //}


      //bool gotData = profile.vertices != null && profile.vertices.Count > 0;
      //var heights = gotData ? (from v in profile.vertices where !float.IsNaN(v.elevation) select v.elevation).ToList() : null;

      //if (heights != null && heights.Count > 0)
      //{
      //  profile.minStation = profile.vertices.Min(v => v.station);
      //  profile.maxStation = profile.vertices.Max(v => v.station);
      //  profile.minHeight = heights.Min();
      //  profile.maxHeight = heights.Max();
      //}
      //else
      //{
      //  profile.minStation = double.NaN;
      //  profile.maxStation = double.NaN;
      //  profile.minHeight = double.NaN;
      //  profile.maxHeight = double.NaN;
      //}

      //return profile;
    }

    private DesignDescriptor GetDescriptor(Guid projectUid, Guid importedFileUid)
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

      var designDescriptor = DesignDescriptor.CreateDesignDescriptor(designFile.LegacyFileId, FileDescriptor.CreateFileDescriptor(GetFilespaceId(), designFile.Path, designFile.Name), 0);

      //var designDescriptor = RaptorConverters.DesignDescriptor(
      //  designFile.LegacyFileId,
      //  designFile.ImportedFileUid,
      //  designFile.Path,
      //  designFile.Name,
      //  0);

      return designDescriptor;
    }

    /// <summary>
    /// Gets the TCC filespaceId for the vldatastore filespace
    /// </summary>
    private string GetFilespaceId()
    {
      var filespaceId = ConfigurationStore.GetValueString("TCCFILESPACEID");
      if (!string.IsNullOrEmpty(filespaceId))
      {
        return filespaceId;
      }

      const string errorString = "Your application is missing an environment variable TCCFILESPACEID";
      Log.LogError(errorString);
      throw new InvalidOperationException(errorString);
    }
  }
}