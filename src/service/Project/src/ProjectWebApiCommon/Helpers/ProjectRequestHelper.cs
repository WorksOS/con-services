using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Extensions;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.WebApi.Common;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public partial class ProjectRequestHelper
  {

    public const double TOLERANCE_DECIMAL_DEGREE = 1e-10;

    /// <summary>
    /// Gets a Project list for customer uid.
    ///  Includes all projects, regardless of archived state and user role
    /// </summary>
    public static async Task<List<ProjectDatabaseModel>> GetProjectListForCustomer(Guid customerUid, Guid? userUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      log.LogDebug($"{nameof(GetProjectListForCustomer)} customerUid {customerUid}, userUid {userUid}");
      var projects = await cwsProjectClient.GetProjectsForCustomer(customerUid, userUid, customHeaders);

      var projectDatabaseModelList = new List<ProjectDatabaseModel>();
      if (projects.Projects != null)
        foreach (var project in projects.Projects)
        {
          if (project.UserProjectRole == UserProjectRoleEnum.Admin)
          {
            var projectDatabaseModel = ConvertCwsToWorksOSProject(project, log);
            if (projectDatabaseModel != null)
              projectDatabaseModelList.Add(projectDatabaseModel);
          }
        }

      log.LogDebug($"{nameof(GetProjectListForCustomer)} Project list contains {projectDatabaseModelList.Count} projects");
      return projectDatabaseModelList;
    }

    /// <summary>
    /// Calibration file is optional for nonThreeDReady projects
    /// cws Filename format is: "trn::profilex:us-west-2:project:5d2ab210-5fb4-4e77-90f9-b0b41c9e6e3f||2020-03-25 23:03:45.314||BootCamp 2012.dc",
    /// </summary>
    public static bool ExtractCalibrationFileDetails(List<ProjectConfigurationModel> projectConfigurations, out string fileName, out DateTime? fileDateUtc)
    {
      fileName = string.Empty;
      fileDateUtc = null;

      var projectConfiguration = projectConfigurations?.FirstOrDefault(c => c.FileType == ProjectConfigurationFileType.CALIBRATION.ToString());
      if (projectConfiguration == null)
        return false;
      var parts = projectConfiguration.FileName.Split(ProjectConfigurationModel.FilenamePathSeparator);
      if (parts.Length == 3)
      {
        fileName = parts[2].Trim();
        var acceptedFileExtensions = new AcceptedFileExtensions();
        if ((!acceptedFileExtensions.IsExtensionAllowed(new List<string> { "dc", "cal" }, fileName))
           || (!DateTime.TryParse(parts[1], out var fileDate)))
          return false;

        fileDateUtc = fileDate;
        return true;
      }

      return false;
    }

    public static ProjectDatabaseModel ConvertCwsToWorksOSProject(ProjectDetailResponseModel project, ILogger log)
    {
      log.LogInformation($"{nameof(ConvertCwsToWorksOSProject)} project {JsonConvert.SerializeObject(project)}");

      var extractedCalibrationFileOk = false;
      var coordinateSystemFileName = string.Empty;
      DateTime? coordinateSystemLastActionedUtc = null;
      if (project.ProjectSettings?.Config!= null && project.ProjectSettings.Config.Any())
         extractedCalibrationFileOk = ExtractCalibrationFileDetails(project.ProjectSettings.Config, out coordinateSystemFileName, out coordinateSystemLastActionedUtc);
      if (project.ProjectSettings?.Boundary == null || project.ProjectSettings?.TimeZone == null)
        log.LogInformation($"{nameof(ConvertCwsToWorksOSProject)} contains no boundary or timezone");
      if (!extractedCalibrationFileOk)
        log.LogInformation($"{nameof(ConvertCwsToWorksOSProject)} contains no calibrationFile.");

      var projectDatabaseModel =
        new ProjectDatabaseModel() 
        {
          ProjectUID = project.ProjectId,
          CustomerUID = project.AccountId,
          Name = project.ProjectName,
          ProjectType = project.ProjectType,
          UserProjectRole = project.UserProjectRole,
          ProjectTimeZone = project.ProjectSettings != null ? PreferencesTimeZones.IanaToWindows(project.ProjectSettings.TimeZone) : string.Empty,
          ProjectTimeZoneIana = project.ProjectSettings?.TimeZone,
          Boundary = project.ProjectSettings?.Boundary != null ? GeometryConversion.ProjectBoundaryToWKT(project.ProjectSettings.Boundary) : string.Empty,
          CoordinateSystemFileName = coordinateSystemFileName,
          CoordinateSystemLastActionedUTC = coordinateSystemLastActionedUtc,
          IsArchived = false, 
          LastActionedUTC = project.LastUpdate ?? DateTime.UtcNow
        };
      return projectDatabaseModel;
    }

    /// <summary>
    /// Gets a Project and checks customerUid
    ///   Includes any project, regardless of archived state.
    ///   Temporary Note that this will return nothing if user doesn't have ADMIN role
    ///       WM team may change this behaviour in future
    /// </summary>
    public static async Task<ProjectDatabaseModel> GetProject(Guid projectUid, Guid customerUid, Guid userUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      var project = await cwsProjectClient.GetMyProject(projectUid, userUid, customHeaders);
      if (project == null)
      {
        log.LogWarning($"Project not found: {projectUid}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
        return null;
      }

      if (!string.Equals(project.AccountId, customerUid.ToString(), StringComparison.OrdinalIgnoreCase))
      {
        log.LogWarning($"Customer doesn't have access to projectUid: {projectUid}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      log.LogInformation($"Project projectUid: {projectUid} retrieved");
      return ConvertCwsToWorksOSProject(project, log);
    }

    /// <summary>
    /// Gets a Project and checks customerUid
    ///   Includes all projects, regardless of archived state and user role
    /// </summary>
    public static async Task<bool> ProjectExists(Guid projectUid, Guid customerUid, Guid userUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      var project = await cwsProjectClient.GetMyProject(projectUid, userUid, customHeaders: customHeaders);
      if (project == null)
      {
        log.LogWarning($"Project not found: {projectUid}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
        return false;
      }

      if (!string.Equals(project.AccountId, customerUid.ToString(), StringComparison.OrdinalIgnoreCase))
      {
        log.LogWarning($"Customer doesn't have access to projectUid: {projectUid}");
        return false;
      }

      log.LogInformation($"Project projectUid: {projectUid} retrieved");
      return true;
    }
    
    /// <summary>
    /// Gets a Project, even if archived.
    ///    Return project even if null. This is called internally from TFA,
    ///      so don't want to throw exception other GetProjects do. Note that no UserUid available.
    ///
    /// Others are called from UI so can throw exception.
    /// </summary>
    public static async Task<ProjectDatabaseModel> GetProjectAndReturn(string projectUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      var project = await cwsProjectClient.GetMyProject(new Guid(projectUid), null, customHeaders: customHeaders);
      if (project == null)
      {
        log.LogInformation($"{nameof(GetProjectAndReturn)} Project projectUid: {projectUid} not retrieved");
        return null;
      }
      log.LogInformation($"{nameof(GetProjectAndReturn)} Project projectUid: {projectUid} project retrieved {JsonConvert.SerializeObject(project)}");
      return ConvertCwsToWorksOSProject(project, log);
    }

    /// <summary>
    /// Gets intersecting projects from cws 
    ///   called from e.g. TFA, so uses applicationContext i.e. no customer. 
    ///   if projectUid is provided, this is a manual import so don't consider itself as potentially overlapping
    /// </summary>
    public static async Task<List<ProjectDatabaseModel>> GetIntersectingProjects(
      string customerUid, double latitude, double longitude, string projectUid, double? northing, double? easting,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      // get projects for customer using application token i.e. no user
      // todo what are the rules e.g. active, for manual import? 
      var projectDatabaseModelList = (await GetProjectListForCustomer(new Guid(customerUid), null,
          log, serviceExceptionHandler, cwsProjectClient, customHeaders))
        .Where(p => string.IsNullOrEmpty(projectUid) || !p.IsArchived);

      /* todoJeannie
      DynamicAddwithOffset("Must contain a location: lat/long or northing/easting", 136);
      DynamicAddwithOffset("Unable to determine lat/long for requested northing/easting", 137);
      DynamicAddwithOffset("A problem occurred attempting to get CSIB for project. Exception: {0}", 138);

        if (!request.HasLatLong)
        {
          var latlongDegrees = await dataRepository.GenerateLatLong(project.ProjectUID, request.Northing.Value, request.Easting.Value);
          if (Math.Abs(latlongDegrees.Lat) < ProjectAndAssetUidsHelper.TOLERANCE_DECIMAL_DEGREE && Math.Abs(latlongDegrees.Lon) < ProjectAndAssetUidsHelper.TOLERANCE_DECIMAL_DEGREE)
            return GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 55);
          request.Latitude = latlongDegrees.Lat;
          request.Longitude = latlongDegrees.Lon;
          log.LogDebug($"{nameof(ProjectAndAssetUidsExecutor)}: Loaded the projects CSIB {JsonConvert.SerializeObject(latlongDegrees)}");
        }
    */

      // return a list at this stage to be used for logging in TFA, but other potential use in future.
      var projects = projectDatabaseModelList.Where(project => !string.IsNullOrEmpty(project.Boundary))
        .Where(project => PolygonUtils.PointInPolygon(project.Boundary, latitude, longitude)).ToList();

      log.LogInformation($"{nameof(GetIntersectingProjects)}: Overlapping projects for customerUid: {customerUid} projects: {JsonConvert.SerializeObject(projects)}");
      return projects;
    }


    /// <summary>
    /// Used by Create/Update project to check if any new boundary overlaps any OTHER project
    /// </summary>
    public static async Task<bool> DoesProjectOverlap(Guid customerUid, Guid? projectUid, Guid userUid, string projectBoundary,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      // get all active projects for customer, excluding this projectUid (i.e. update)
      // todo what are the rules e.g. active, for manual import?

      var projectDatabaseModelList = (await GetProjectListForCustomer(customerUid, userUid,
          log, serviceExceptionHandler, cwsProjectClient, customHeaders))
        .Where(p => !p.IsArchived && p.ProjectType.HasFlag(CwsProjectType.AcceptsTagFiles) &&
                    (projectUid == null || string.Compare(p.ProjectUID.ToString(), projectUid.ToString(), StringComparison.OrdinalIgnoreCase) != 0));

      // return once we find any overlapping projects
      foreach (var project in projectDatabaseModelList)
      {
        if (string.IsNullOrEmpty(project.Boundary)) continue;
        if (PolygonUtils.OverlappingPolygons(projectBoundary, project.Boundary))
          return true;
      }

      log.LogDebug($"{nameof(DoesProjectOverlap)}: No overlapping projects.");
      return false; 
    }

    /// <summary>
    /// Convert projects northing/easting to a lat/long
    ///   obtains projects CSIB to call NE-->LL conversion
    ///  Note: this comes from a v2 manual import and assumes project already found and ok so far
    /// </summary>
    public async Task<WGSPoint> GenerateLatLong(string projectUid, double northing, double easting)
    {
      var projectCSIB = await GetCSIBFromTRex(projectUid);

      var northingEasting = new WGSPoint(northing, easting); // todoJeannie Aaron to establish new NEE class
      var latLongDegrees = new WGSPoint(0, 0); // 0,0 is invalid lat/long
      if (!string.IsNullOrEmpty(projectCSIB))
      {
        //todoJeannie latLongDegrees = AaronsNewConvertCoordinates.NEEToLLH(projectCSIB, northingEasting);
        latLongDegrees = new WGSPoint(50, 50);
      }

      return latLongDegrees;
    }


    /// <summary>
    /// Get CSIB/s for a project
    ///    this is cached in proxy
    /// </summary>
    public async Task<string> GetCSIBFromTRex(string projectUid)
    {
      return string.Empty;
      // todo via ProjectSv --> 3dp --> Trex
      //try
      //{
      //  var returnResult = await _tRexCompactionDataProxy.SendDataGetRequest<CSIBResult>(projectUid, $"/projects/{projectUid}/csib", _customHeaders, isCachingRequired: true);
      //  return returnResult.CSIB;
      //}
      //catch (Exception e)
      //{
      //  throw new ServiceException(HttpStatusCode.InternalServerError,
      //    TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
      //      ContractExecutionStatesEnum.InternalProcessingError, 53, e.Message));
      //}
    }

    #region coordSystem

    /// <summary>
    /// validate CoordinateSystem if provided
    /// </summary>
    public static async Task<bool> ValidateCoordSystemInProductivity3D(string csFileName, byte[] csFileContent,
      IServiceExceptionHandler serviceExceptionHandler, IHeaderDictionary customHeaders,
      IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord)
    {
      if (!string.IsNullOrEmpty(csFileName) || csFileContent != null)
      {
        ProjectDataValidator.ValidateFileName(csFileName);
        CoordinateSystemSettingsResult coordinateSystemSettingsResult = null;
        try
        {
          coordinateSystemSettingsResult = await productivity3dV1ProxyCoord
            .CoordinateSystemValidate(csFileContent, csFileName, customHeaders);
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,
            "productivity3dV1ProxyCoord.CoordinateSystemValidate", e.Message);
        }

        if (coordinateSystemSettingsResult == null)
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 46);

        if (coordinateSystemSettingsResult != null &&
            coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 47,
            coordinateSystemSettingsResult.Code.ToString(),
            coordinateSystemSettingsResult.Message);
        }
      }

      return true;
    }

    /// <summary>
    /// Create CoordinateSystem in Raptor and save a copy of the file in DataOcean
    /// </summary>
    ///  todo CCSSSCON-351 cleanup parameters once UpdateProject endpoint has been converted
    public static async Task CreateCoordSystemInProductivity3dAndTcc(Guid projectUid,
      string coordinateSystemFileName,
      byte[] coordinateSystemFileContent, bool isCreate,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, string customerUid,
      IHeaderDictionary customHeaders,
      IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord, IConfigurationStore configStore,
      IDataOceanClient dataOceanClient, ITPaaSApplicationAuthentication authn,
      ICwsDesignClient cwsDesignClient, ICwsProfileSettingsClient cwsProfileSettingsClient, ICwsProjectClient cwsProjectClient = null)
    {
      if (!string.IsNullOrEmpty(coordinateSystemFileName))
      {
        var headers = customHeaders;
        headers.TryGetValue("X-VisionLink-ClearCache", out var caching);
        if (string.IsNullOrEmpty(caching)) // may already have been set by acceptance tests
          headers.Add("X-VisionLink-ClearCache", "true");

        try
        {
          //Pass coordinate system to Raptor
          CoordinateSystemSettingsResult coordinateSystemSettingsResult;
          coordinateSystemSettingsResult = await productivity3dV1ProxyCoord
            .CoordinateSystemPost(projectUid,
              coordinateSystemFileContent, coordinateSystemFileName, headers);
          var message = string.Format($"Post of CS create to RaptorServices returned code: {0} Message {1}.",
            coordinateSystemSettingsResult?.Code ?? -1,
            coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
          log.LogDebug(message);
          if (coordinateSystemSettingsResult == null ||
              coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
          {
            if (isCreate)
              await RollbackProjectCreation(Guid.Parse(customerUid), projectUid, log, cwsProjectClient);

            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 41,
              (coordinateSystemSettingsResult?.Code ?? -1).ToString(),
              coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
          }

          //save copy to DataOcean
          var rootFolder = configStore.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
          if (string.IsNullOrEmpty(rootFolder))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 115);
          }

          using (var ms = new MemoryStream(coordinateSystemFileContent))
          {
            await DataOceanHelper.WriteFileToDataOcean(
              ms, rootFolder, customerUid, projectUid.ToString(),
              DataOceanFileUtil.DataOceanFileName(coordinateSystemFileName, false, projectUid, null),
              log, serviceExceptionHandler, dataOceanClient, authn, projectUid, configStore);
          }
        }
        catch (Exception e)
        {
          if (isCreate)
            await RollbackProjectCreation(Guid.Parse(customerUid), projectUid, log, cwsProjectClient);

          //Don't hide exceptions thrown above
          if (e is ServiceException)
            throw;
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "productivity3dV1ProxyCoord.CoordinateSystemPost", e.Message);
        }
      }
    }

    #endregion coordSystem


    #region S3
    /// <summary>
    /// Writes the importedFile to S3
    ///   if file exists, it will be overwritten
    ///   returns FileDescriptor for backwards compatability
    /// </summary>
    /// <returns></returns>
    public static FileDescriptor WriteFileToS3Repository(
      Stream fileContents, string projectUid, string filename,
      bool isSurveyedSurface, DateTime? surveyedUtc,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      ITransferProxy persistantTransferProxy)
    {
      string finalFilename = filename;
      if (isSurveyedSurface && surveyedUtc != null) // validation should prevent this
        finalFilename = finalFilename.IncludeSurveyedUtcInName(surveyedUtc.Value);

      var s3FullPath = $"{projectUid}/{finalFilename}";
      try
      {
        persistantTransferProxy.Upload(fileContents, s3FullPath);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "transferProxy.Upload()",
          e.Message);
      }

      log.LogInformation($"WriteFileToS3Repository. Process add design :{finalFilename}, for Project:{projectUid}");
      return FileDescriptor.CreateFileDescriptor(string.Empty, string.Empty, finalFilename);
    }
    #endregion S3

    #region rollback

    /// <summary>
    /// Used internally, if a step fails, after a project has been CREATED, 
    ///    then  what to do - delete from cws?
    ///    CCSSSCON-417
    /// </summary>
    private static async Task RollbackProjectCreation(Guid customerUid, Guid projectUid, ILogger log,
      ICwsProjectClient projectClient)
    {
      log.LogError($"RollbackProjectCreation: NOT IMPLEMENTED YET customerUid {customerUid} projectUid {projectUid}");
    }

    #endregion rollback

  }
}
