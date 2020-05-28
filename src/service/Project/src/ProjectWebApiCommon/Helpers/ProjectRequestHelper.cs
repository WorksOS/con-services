using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
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
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Coord.ResultHandling;
using VSS.TCCFileAccess;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public partial class ProjectRequestHelper
  {

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
          var projectDatabaseModel = ConvertCwsToWorksOSProject(project, log);
          if (projectDatabaseModel != null)
            projectDatabaseModelList.Add(projectDatabaseModel);
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
          ProjectType = ProjectType.Standard,
          ProjectTimeZone = project.ProjectSettings != null ? PreferencesTimeZones.IanaToWindows(project.ProjectSettings.TimeZone) : string.Empty,
          ProjectTimeZoneIana = project.ProjectSettings?.TimeZone,
          Boundary = project.ProjectSettings?.Boundary != null ? RepositoryHelper.ProjectBoundaryToWKT(project.ProjectSettings.Boundary) : string.Empty,
          CoordinateSystemFileName = coordinateSystemFileName,
          CoordinateSystemLastActionedUTC = coordinateSystemLastActionedUtc,
          IsArchived = false, 
          LastActionedUTC = project.LastUpdate
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
    /// Gets intersecting projects in localDB . applicationContext i.e. no customer. 
    ///   if projectUid, get it if it overlaps in localDB
    ///    else get overlapping projects in localDB for this CustomerUID
    /// </summary>
    public static async Task<List<ProjectDatabaseModel>> GetIntersectingProjects(
      string customerUid, double latitude, double longitude, string projectUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      // get projects for customer using application token i.e. no user
      // todo what are the rules e.g. active, for manual import? 
      var projectDatabaseModelList = (await GetProjectListForCustomer(new Guid(customerUid), null,
          log, serviceExceptionHandler, cwsProjectClient, customHeaders))
        .Where(p => string.IsNullOrEmpty(projectUid) || !p.IsArchived); 

      var projects = new List<ProjectDatabaseModel>();
      // call new overlap routine  // todo CCSSSCON-341
      //projects =
      //  await Wherever.GetIntersectingProjects(latitude, longitude, projectDatabaseModelList);

      log.LogInformation($"Projects for customerUid: {customerUid} count: {projects.Count}");
      return projects;
    }


    public static async Task<bool> DoesProjectOverlap(Guid customerUid, Guid? projectUid, Guid userUid, string projectBoundary,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      // get all active projects for customer, excluding this projectUid (i.e. update)
      var projectDatabaseModelList = (await GetProjectListForCustomer(customerUid, userUid,
          log, serviceExceptionHandler, cwsProjectClient, customHeaders))
        .Where(p => !p.IsArchived &&
                    (projectUid == null || string.Compare(p.ProjectUID.ToString(), projectUid.ToString(), StringComparison.OrdinalIgnoreCase) != 0));

      // call new overlap routine // todo CCSSSCON-341
      //var overlaps =
      //  await Wherever.DoesPolygonOverlap(projectBoundary, projectDatabaseModelList);
      //if (overlaps)
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 43);

      log.LogDebug($"No overlapping projects for: {projectUid}");
      return false; // todo CCSSSCON-341
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
    /// Create CoordinateSystem in Raptor and save a copy of the file in TCC
    /// </summary>
    ///  todo CCSSSCON-351 cleanup parameters once UpdateProject endpoint has been converted
    public static async Task CreateCoordSystemInProductivity3dAndTcc(Guid projectUid,
      string coordinateSystemFileName,
      byte[] coordinateSystemFileContent, bool isCreate,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, string customerUid,
      IHeaderDictionary customHeaders,
      IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord, IConfigurationStore configStore,
      IFileRepository fileRepo, IDataOceanClient dataOceanClient, ITPaaSApplicationAuthentication authn,
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

          //and save copy of file in TCC
          var fileSpaceId = configStore.GetValueString("TCCFILESPACEID");
          if (string.IsNullOrEmpty(fileSpaceId))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48);
          }

          using (var ms = new MemoryStream(coordinateSystemFileContent))
          {
            await TccHelper.WriteFileToTCCRepository(
              ms, customerUid, projectUid.ToString(), coordinateSystemFileName,
              false, null, fileSpaceId, log, serviceExceptionHandler, fileRepo);
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

          //save to CWS
          using (var ms = new MemoryStream(coordinateSystemFileContent))
          {
            //TODO: handle errors from CWS
            await CwsConfigFileHelper.SaveFileToCws(projectUid, coordinateSystemFileName, ms, ImportedFileType.CwsCalibration,
              cwsDesignClient, cwsProfileSettingsClient, customHeaders);
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
