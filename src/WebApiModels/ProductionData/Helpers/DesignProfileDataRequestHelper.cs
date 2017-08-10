using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using SVOICFilterSettings;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
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
      this.ConfigurationStore = configurationStore;
      this.FileListProxy = fileListProxy;
      this.SettingsManager = settingsManager;
    }
    public DesignProfileDataRequestHelper SetRaptorClient(IASNodeClient raptorClient)
    {
      _raptorClient = raptorClient;
      return this;
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
    public ProfileResult CreateDesignProfileResponse(Guid projectUid, double latRadians1, double lngRadians1, double latRadians2, double lngRadians2, long designId, string designFilename, int importedFileTypeid, bool alignmentProfile, long alignmentId, double startStation, double endStation, string callId)
    {
      TVLPDDesignDescriptor alignmentDescriptor;

      if (alignmentProfile)
      {
        lngRadians1 = 0;
        latRadians1 = 0;
        lngRadians2 = 0;
        latRadians2 = 0;

        alignmentDescriptor = GetAlignmentDescriptor(importedFileTypeid, designId, designFilename);
      }
      else
      {
        alignmentDescriptor = RaptorConverters.EmptyDesignDescriptor;
      }

      TVLPDDesignDescriptor designDescriptor;

      if (importedFileTypeid == (int)ImportedFileType.ReferenceSurface)
      {
        designDescriptor = RaptorConverters.DesignDescriptor(designId, ProjectId.ToString(), null, null, 0); // Returns parent file plus offset.
      }
      else
      {
      
        // TCCHelper.TCCData data = TCCHelper.LoginToBusinessCenterAsVL(session, projectID);

        //designDescriptor = RaptorConverters.DesignDescriptor(
        //  designID,
        //  null, // shoudln't be null.
        //  data.filePath,
        //  designFilename,
        //  0);
      }

      MemoryStream ms = null;
      //int code = _raptorClient.GetDesignProfile(DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args
      //  (ProjectId, alignmentProfile,
      //    VLPDDecls.TWGS84Point.Point(lngRadians1, latRadians1),
      //    VLPDDecls.TWGS84Point.Point(lngRadians2, latRadians2),
      //    startStation, endStation,
      //    designDescriptor,
      //    alignmentDescriptor,
      //    null, // no filter yet.
      //    false),
      //  out MS);


      //VLPDDecls.TWGS84Point startPt, endPt;
      //bool positionsAreGrid;
      //ProfilesHelper.convertProfileEndPositions(request.gridPoints, request.wgs84Points, out startPt, out endPt, out positionsAreGrid);


      //ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args(ProjectId,
      //  -1,
      //  true,
      //  startPt,
      //  endPt,
      //  null,
      //   null,
      //  RaptorConverters.DesignDescriptor(alignmentDescriptor.DesignID, alignmentDescriptor.FileSpaceID, alignmentDescriptor.FullPath(), alignmentDescriptor.FileName, 0),
      //  true);

      //var ms = _raptorClient.GetProfile(args);

      //int code = _raptorClient.GetDesignProfile(DesignProfiler.ComputeProfile.RPC.__Global.Construct_CalculateDesignProfile_Args
      //  (ProjectId, alignmentProfile,
      //    TWGS84Point.Point(lngRadians1, latRadians1),
      //    TWGS84Point.Point(lngRadians2, latRadians2),
      //    startStation,
      //    endStation,
      //    designDescriptor,
      //    alignmentDescriptor,
      //    null, // no filterget
      //    false),
      //  out MemoryStream memoryStream);

      //if (memoryStream != null)
      //{
      //  var result = ProfilesHelper.convertProductionDataProfileResult(memoryStream, (Guid)(callId ?? Guid.NewGuid()));
      //  log.LogInformation("GetProfileProduction result: " + JsonConvert.SerializeObject(result));

      //  return result;
      //}

      return null;
    }

    private TVLPDDesignDescriptor GetAlignmentDescriptor(int importedFileTypeid, long designId, string designFilename)
    {
      var fileList = FileListProxy.GetFiles(ProjectId.ToString(), Headers).Result;

      if (fileList.Count <= 0)
      {
        //throw new ServiceException(HttpStatusCode.BadRequest,
        //  new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
        //    "Project has no appropriate design files."));
      }

      // Is this query correct?
      var designFile = fileList.SingleOrDefault(
        f => f.ImportedFileUid ==
             importedFileTypeid.ToString() &&
             f.IsActivated &&
             f.ImportedFileType == ImportedFileType.DesignSurface);

      if (designFile == null)
      {
        //throw new ServiceException(HttpStatusCode.BadRequest,
        //  new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
        //    "Unable to access design file."));
      }

      var alignmentDescriptor = RaptorConverters.DesignDescriptor(
        designId,
        designFile.ImportedFileUid,
        designFile.Path,
        designFilename,
        0);

      return alignmentDescriptor;
    }
  }
}