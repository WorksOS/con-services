﻿using System;
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
using VSS.Common.Abstractions.Extensions;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.ExtendedModels;
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
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, CwsProjectType? projectType, 
      ProjectStatus? status, bool onlyAdmin, bool includeBoundaries, IHeaderDictionary customHeaders)
    {
      log.LogDebug($"{nameof(GetProjectListForCustomer)} customerUid {customerUid}, userUid {userUid}");
      var projects = await cwsProjectClient.GetProjectsForCustomer(customerUid, userUid, includeBoundaries, projectType, status, onlyAdmin, customHeaders);

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
    /// Gets a Project for a shortProjectId for TBC
    ///  Regardless of archived state and user role
    /// </summary>
    public static async Task<ProjectDetailResponseModel> GetProjectForCustomer(Guid customerUid, Guid? userUid, long projectShortId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      log.LogDebug($"{nameof(GetProjectForCustomer)} customerUid {customerUid}, userUid {userUid} projectShortId {projectShortId}");
      var projects = await cwsProjectClient.GetProjectsForCustomer(customerUid, userUid,
        type: CwsProjectType.AcceptsTagFiles, customHeaders: customHeaders);

      var projectMatches = projects.Projects.Where(p => (Guid.TryParse(p.ProjectId, out var g) ? g.ToLegacyId() : 0) == projectShortId).ToList();
      log.LogDebug($"{nameof(GetProjectForCustomer)} Found {projectMatches.Count} projects");
      if (projectMatches.Count != 1)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (projectMatches.Count == 0 ? 1 : 139));

      log.LogDebug($"{nameof(GetProjectForCustomer)} Project matched {JsonConvert.SerializeObject(projectMatches[0])}");
      return projectMatches[0];
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
      if (project.ProjectSettings?.Config != null && project.ProjectSettings.Config.Any())
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
          IsArchived = project.Status == ProjectStatus.Archived,
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
      string customerUid, double latitude, double longitude, string projectUid,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ICwsProjectClient cwsProjectClient, IHeaderDictionary customHeaders)
    {
      // get projects for customer using application token i.e. no user
      // todo what are the rules e.g. active, for manual import? 
      var projectDatabaseModelList = (await GetProjectListForCustomer(new Guid(customerUid), null,
          log, serviceExceptionHandler, cwsProjectClient, CwsProjectType.AcceptsTagFiles, ProjectStatus.Active, false, true, customHeaders))
        .Where(p => string.IsNullOrEmpty(projectUid) || !p.IsArchived);

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
          log, serviceExceptionHandler, cwsProjectClient, CwsProjectType.AcceptsTagFiles, ProjectStatus.Active, false, true, customHeaders))
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
      var s3FullPath = GetS3FullPath(projectUid, filename, isSurveyedSurface, surveyedUtc);
      try
      {
        persistantTransferProxy.Upload(fileContents, s3FullPath);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "transferProxy.Upload()",
          e.Message);
      }

      log.LogInformation($"WriteFileToS3Repository. Process add design :{s3FullPath}, for Project:{projectUid}");
      var finalFilename = s3FullPath.Split("/")[1];
      return FileDescriptor.CreateFileDescriptor(string.Empty, string.Empty, finalFilename);
    }

    /// <summary>
    /// Deletes the imported file from S3.
    /// </summary>
    public static void DeleteFileFromS3Repository(string projectUid, string filename,
      bool isSurveyedSurface, DateTime? surveyedUtc,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      ITransferProxy persistantTransferProxy)
    {
      var s3FullPath = GetS3FullPath(projectUid, filename, isSurveyedSurface, surveyedUtc);
      try
      {
        persistantTransferProxy.RemoveFromBucket(s3FullPath);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "transferProxy.RemoveFromBucket()",
          e.Message);
      }

      log.LogInformation($"DeleteFileFromS3Repository. Process delete design :{s3FullPath}, for Project:{projectUid}");
    }

    private static string GetS3FullPath(string projectUid, string filename,
      bool isSurveyedSurface, DateTime? surveyedUtc)
    {
      var finalFilename = filename;
      if (isSurveyedSurface && surveyedUtc != null) // validation should prevent this
        finalFilename = finalFilename.IncludeSurveyedUtcInName(surveyedUtc.Value);

      return $"{projectUid}/{finalFilename}";
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
