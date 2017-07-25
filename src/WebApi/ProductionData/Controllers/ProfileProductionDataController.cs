using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;
using VSS.Productivity3D.Common.ResultHandling;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterDataProxies;
using VSS.MasterDataProxies.Interfaces;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for the ProfileProductionData resource.
  /// </summary>
  /// 
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] {"*"})]
  public class ProfileProductionDataController : Controller, IProfileProductionDataContract
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private IConfigurationStore configStore;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// Constructor with injected raptor client and logger
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="configStore"></param>
    /// <param name="fileListProxy"></param>
    public ProfileProductionDataController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore, IFileListProxy fileListProxy)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<ProfileProductionDataController>();
      this.configStore = configStore;
      this.fileListProxy = fileListProxy;
    }


    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="request">Profile production data request structure.></param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations. {"Code":0,"Message":"User-friendly"}
    /// List of codes:
    ///     OK = 0,
    ///     Incorrect Requested Data = -1,
    ///     Validation Error = -2
    ///     InternalProcessingError = -3;
    ///     FailedToGetResults = -4;
    /// </returns>
    /// <executor>ProfileProductionDataExecutor</executor>
    /// 
    [PostRequestVerifier]
    [ProjectIdVerifier]
    [NotLandFillProjectVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v1/profiles/productiondata")]
    [HttpPost]
    public ProfileResult Post([FromBody] ProfileProductionDataRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<ProfileProductionDataExecutor>(logger, raptorClient, null)
        .Process(request) as ProfileResult;
    }

    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startLatDegrees">Start profileLine Lat</param>
    /// <param name="startLonDegrees">Start profileLine Lon</param>
    /// <param name="endLatDegrees">End profileLine Lat</param>
    /// <param name="endLonDegrees">End profileLine Lon</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="cutfillDesignUid">Design UID</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations. {"Code":0,"Message":"User-friendly"}
    /// List of codes:
    ///     OK = 0,
    ///     Incorrect Requested Data = -1,
    ///     Validation Error = -2
    ///     InternalProcessingError = -3;
    ///     FailedToGetResults = -4;
    /// </returns>
    /// <executor>ProfileProductionDataExecutor</executor>
    [PostRequestVerifier]
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier] 
    [Route("api/v2/profiles/productiondata/slicer")]
    [HttpGet]
    public async Task<ProfileResult> GetProfileProductionDataSlicer(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] Guid? cutfillDesignUid
    )
    {
      log.LogInformation("GetProfileProduction: " + Request.QueryString);
      ProfileProductionDataRequest request = await GetProfileProductionRequest(projectUid,
          startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees, startUtc, endUtc, cutfillDesignUid);
      request.Validate();

      try
      {
        var result = RequestExecutorContainer.Build<ProfileProductionDataExecutor>(logger, raptorClient, null)
          .Process(request) as ProfileResult;
        log.LogInformation("GetProfileProduction result: " + JsonConvert.SerializeObject(result));
        return result;
      }
      catch (ServiceException se)
      {
        //Change FailedToGetResults to 204
        this.ProcessStatusCode(se);
        throw;
      }
      finally
      {
        log.LogInformation("GetProfileProduction returned: " + Response.StatusCode);
      }
    }

    /*
     *  [Route("api/v2/profiles/productiondata/alignment")]
    [HttpGet]
    public async Task<ProfileResult> GetProfileProductionDataAlignment(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] DateTime? startUtc,
      [FromQuery] DateTime? endUtc,
      [FromQuery] Guid? cutfillDesignUid
    // these are alignment and only doing slicer at this stage
    // [FromQuery] bool alignmentProfile, 
    // [FromQuery] long designID, 
    // [FromQuery] double startStation, 
    // [FromQuery] double endStation,
    */


    #region privates

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
    private async Task<ProfileProductionDataRequest> GetProfileProductionRequest(Guid projectUid, 
      double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees, 
      DateTime? startUtc, DateTime? endUtc, Guid? cutfillDesignUid)
    {
      long projectId = (User as RaptorPrincipal).GetProjectId(projectUid);

      ProfileLLPoints llPoints = ProfileLLPoints.CreateProfileLLPoints(startLatDegrees.latDegreesToRadians(), startLonDegrees.lonDegreesToRadians(), endLatDegrees.latDegreesToRadians(), endLonDegrees.lonDegreesToRadians()); 
      
      ProductionDataType profileType = ProductionDataType.Height;

      var excludedIds = await this.GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid, Request.Headers.GetCustomHeaders());
      Filter filter = CompactionSettings.CompactionFilter(startUtc, endUtc, null, null, null, null, null, excludedIds);

      DesignDescriptor designDescriptor = null;
      if (cutfillDesignUid.HasValue)
      {
        var fileList = await fileListProxy.GetFiles(projectUid.ToString(), Request.Headers.GetCustomHeaders());

        if (fileList.Count > 0)
        {
          var designFile = fileList?.Where(f => f.ImportedFileUid == cutfillDesignUid.Value.ToString() &&
                                                f.IsActivated &&
                                                f.ImportedFileType == ImportedFileType.DesignSurface).SingleOrDefault();

          if (designFile != null)
          {
            designDescriptor = DesignDescriptor.CreateDesignDescriptor(designFile.LegacyFileId, FileDescriptor.CreateFileDescriptor(GetFilespaceId(), designFile.Path, designFile.Name), 0);
          }
          else 
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
                "Unable to access design file."));
          }
          
        }
        else 
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
              "Project has no appropriate design files."));
        }
      } 

      LiftBuildSettings liftBuildSettings = CompactionSettings.CompactionLiftBuildSettings;

      return ProfileProductionDataRequest.CreateProfileProductionData(projectId, null, profileType, filter, -1,
        designDescriptor, null, llPoints, ValidationConstants.MIN_STATION, ValidationConstants.MIN_STATION, liftBuildSettings, false);
    }


    /// <summary>
    /// Gets the TCC filespaceId for the vldatastore filespace
    /// </summary>
    /// <returns></returns>
    private string GetFilespaceId()
    {
      string filespaceId = configStore.GetValueString("TCCFILESPACEID");
      if (string.IsNullOrEmpty(filespaceId))
      {
        var errorString = "Your application is missing an environment variable TCCFILESPACEID";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
      return filespaceId;
    }
    
    #endregion privates
  }
}
