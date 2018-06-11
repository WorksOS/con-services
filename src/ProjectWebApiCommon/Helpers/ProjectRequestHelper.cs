using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public class ProjectRequestHelper
  {
    /// <summary>
    /// validate CordinateSystem if provided
    /// </summary>
    public static async Task<bool> ValidateCoordSystemInRaptor(IProjectEvent project,
      IServiceExceptionHandler serviceExceptionHandler, IDictionary<string, string> customHeaders,
      IRaptorProxy raptorProxy)
    {
      // a Creating a landfill must have a CS, else optional
      //  if updating a landfill, or other then May have one. Note that a null one doesn't overwrite any existing.
      if (project is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent) project;
        if (projectEvent.ProjectType == ProjectType.LandFill
            && (string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName)
                || projectEvent.CoordinateSystemFileContent == null)
        )
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 45);
      }

      var csFileName = project is CreateProjectEvent
        ? ((CreateProjectEvent) project).CoordinateSystemFileName
        : ((UpdateProjectEvent) project).CoordinateSystemFileName;
      var csFileContent = project is CreateProjectEvent
        ? ((CreateProjectEvent) project).CoordinateSystemFileContent
        : ((UpdateProjectEvent) project).CoordinateSystemFileContent;
      if (!string.IsNullOrEmpty(csFileName) || csFileContent != null)
      {
        ProjectDataValidator.ValidateFileName(csFileName);
        CoordinateSystemSettingsResult coordinateSystemSettingsResult = null;
        try
        {
          coordinateSystemSettingsResult = await raptorProxy
            .CoordinateSystemValidate(csFileContent, csFileName, customHeaders)
            .ConfigureAwait(false);
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,
            "raptorProxy.CoordinateSystemValidate", e.Message);
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

    public static void ValidateGeofence(string projectBoundary, IServiceExceptionHandler serviceExceptionHandler)
    {
      var result = GeofenceValidation.ValidateWKT(projectBoundary);
      if (String.CompareOrdinal(result, GeofenceValidation.ValidationOk) != 0)
      {
        if (String.CompareOrdinal(result, GeofenceValidation.ValidationNoBoundary) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 23);
        }

        if (String.CompareOrdinal(result, GeofenceValidation.ValidationLessThan3Points) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 24);
        }

        if (String.CompareOrdinal(result, GeofenceValidation.ValidationInvalidFormat) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 25);
        }
      }
    }

    public static async Task<bool> DoesProjectOverlap(string customerUid, string projectUid, DateTime projectStartDate, DateTime projectEndDate, string databaseProjectBoundary,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var overlaps =
        await projectRepo.DoesPolygonOverlap(customerUid, databaseProjectBoundary,
          projectStartDate, projectEndDate).ConfigureAwait(false);
      if (overlaps)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 43);

      log.LogDebug($"No overlapping projects for: {projectUid}");
      return overlaps;
    }

    /// <summary>
    /// Create CoordinateSystem in Raptor and save a copy of the file in TCC
    /// </summary>
    public static async Task CreateCoordSystemInRaptorAndTcc(Guid projectUid, int legacyProjectId, string coordinateSystemFileName,
      byte[] coordinateSystemFileContent, bool isCreate,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, string customerUid, IDictionary<string, string> customHeaders, 
      IProjectRepository projectRepo, IRaptorProxy raptorProxy, IConfigurationStore configStore, IFileRepository fileRepo)
    {
      if (!string.IsNullOrEmpty(coordinateSystemFileName))
      {
        var headers = customHeaders;
        headers.TryGetValue("X-VisionLink-ClearCache", out string caching);
        if (string.IsNullOrEmpty(caching)) // may already have been set by acceptance tests
          headers.Add("X-VisionLink-ClearCache", "true");

        try
        {
          //Pass coordinate system to Raptor
          var coordinateSystemSettingsResult = await raptorProxy
            .CoordinateSystemPost(legacyProjectId, coordinateSystemFileContent,
              coordinateSystemFileName, headers).ConfigureAwait(false);
          var message = string.Format($"Post of CS create to RaptorServices returned code: {0} Message {1}.",
            coordinateSystemSettingsResult?.Code ?? -1,
            coordinateSystemSettingsResult?.Message ?? "coordinateSystemSettingsResult == null");
          log.LogDebug(message);
          if (coordinateSystemSettingsResult == null ||
              coordinateSystemSettingsResult.Code != 0 /* TASNodeErrorStatus.asneOK */)
          {
            if (isCreate)
              await ProjectRequestHelper.DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid, log, projectRepo).ConfigureAwait(false);

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
            var fileDescriptor = await ProjectRequestHelper.WriteFileToTCCRepository(
                ms, customerUid, projectUid.ToString(), coordinateSystemFileName,
                false, null, fileSpaceId, log, serviceExceptionHandler, fileRepo)
              .ConfigureAwait(false);
          }

        }
        catch (Exception e)
        {
          if (isCreate)
            await ProjectRequestHelper.DeleteProjectPermanentlyInDb(Guid.Parse(customerUid), projectUid, log, projectRepo).ConfigureAwait(false);

          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57,
            "raptorProxy.CoordinateSystemPost", e.Message);
        }
      }
    }

    /// <summary>
    /// get file content from TCC
    ///     note that is is intended to be used for small, DC files only.
    ///     If/when it is needed for large files, 
    ///           e.g. surfaces, you should use a smaller buffer and loop to read.
    /// </summary>
    public static async Task<byte[]> GetFileContentFromTcc(BusinessCenterFile businessCentreFile,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      Stream memStream = null;
      var tccPath = $"{businessCentreFile.Path}/{businessCentreFile.Name}";
      byte[] coordSystemFileContent = null;
      int numBytesRead = 0;

      try
      {
        log.LogInformation(
          $"GetFileContentFromTcc: getBusinessCentreFile fielspaceID: {businessCentreFile.FileSpaceId} tccPath: {tccPath}");
        memStream = await fileRepo.GetFile(businessCentreFile.FileSpaceId, tccPath).ConfigureAwait(false);

        if (memStream != null && memStream.CanRead && memStream.Length > 0)
        {
          coordSystemFileContent = new byte[memStream.Length];
          int numBytesToRead = (int)memStream.Length;
          numBytesRead = memStream.Read(coordSystemFileContent, 0, numBytesToRead);
        }
        else
        {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
              80, $" isAbleToRead: {memStream != null && memStream.CanRead} bytesReturned: {memStream?.Length ?? 0}");
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 79, e.Message);
      }
      finally
      {
        memStream?.Dispose();
      }

      log.LogInformation(
        $"GetFileContentFromTcc: numBytesRead: {numBytesRead} coordSystemFileContent.Length {coordSystemFileContent?.Length ?? 0}");
      return coordSystemFileContent;
    }

    /// <summary>
    /// Writes the importedFile to TCC
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists already
    /// </summary>
    /// <returns></returns>
    public static async Task<FileDescriptor> WriteFileToTCCRepository(
      Stream fileContents, string customerUid, string projectUid,
      string pathAndFileName, bool isSurveyedSurface, DateTime? surveyedUtc, string fileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      var tccPath = $"/{customerUid}/{projectUid}";
      string tccFileName = Path.GetFileName(pathAndFileName);

      if (isSurveyedSurface && surveyedUtc != null) // validation should prevent this
          tccFileName = ImportedFileUtils.IncludeSurveyedUtcInName(tccFileName, surveyedUtc.Value);

      bool ccPutFileResult = false;
      bool folderAlreadyExists = false;
      try
      {
        log.LogInformation(
          $"WriteFileToTCCRepository: fileSpaceId {fileSpaceId} tccPath {tccPath} tccFileName {tccFileName}");
        // check for exists first to avoid an misleading exception in our logs.
        folderAlreadyExists = await fileRepo.FolderExists(fileSpaceId, tccPath).ConfigureAwait(false);
        if (folderAlreadyExists == false)
          await fileRepo.MakeFolder(fileSpaceId, tccPath).ConfigureAwait(false);

        // this does an upsert
        ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPath, tccFileName, fileContents, fileContents.Length)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.PutFile",
          e.Message);
      }

      if (ccPutFileResult == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 53);
      }

      log.LogInformation(
        $"WriteFileToTCCRepository: tccFileName {tccFileName} written to TCC. folderAlreadyExists {folderAlreadyExists}");
      return FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPath, tccFileName);
    }

    /// <summary>
    /// Gets the geofence list available for a customer, 
    ///    or those associated with a project
    /// </summary>
    /// <returns></returns>
    public static async Task<List<GeofenceWithAssociation>> GetGeofenceList(string customerUid, string projectUid, int[] geofenceTypes,
      ILogger log, IProjectRepository projectRepo, IGeofenceRepository geofenceRepo)
    {
      log.LogInformation(
        $"GetGeofenceList: customerUid {customerUid}, projectUid {projectUid}, {JsonConvert.SerializeObject(geofenceTypes)}");

      var geofencesWithAssociation = (await projectRepo.GetCustomerGeofences(customerUid).ConfigureAwait(false))
        .Where(g => !geofenceTypes.Any() || geofenceTypes.Contains((int) g.GeofenceType));

      if (!string.IsNullOrEmpty(projectUid))
      {
        var geofencesAssociated = geofencesWithAssociation
          .Where(g => g.ProjectUID == projectUid).ToList();

        var associated = geofencesAssociated.ToList();
        log.LogInformation(
          $"Geofence list contains {associated.Count} geofences associated to project {projectUid}");
        return associated;
      }

      // geofences which are not associated with ANY project
      var notAssociated = geofencesWithAssociation
        .Where(g => string.IsNullOrEmpty(g.ProjectUID)).ToList();
      log.LogInformation($"Geofence list contains {notAssociated.Count} available geofences");
      return notAssociated;
    }

    //public static async Task<List<Geofence>> GetGeofenceList(string customerUid, string projectUid, int[] geofenceTypes,
    //  ILogger log, IProjectRepository projectRepo, IGeofenceRepository geofenceRepo)
    //{
    //  log.LogInformation(
    //    $"GetGeofenceList: customerUid {customerUid}, projectUid {projectUid}, {JsonConvert.SerializeObject(geofenceTypes)}");

    //  var geofences = (await geofenceRepo.GetCustomerGeofences(customerUid).ConfigureAwait(false))
    //    .Where(g => !geofenceTypes.Any() || geofenceTypes.Contains((int)g.GeofenceType));

    //  if (!string.IsNullOrEmpty(projectUid))
    //  {
    //    // remove any of wrong type, or for which the Geofence has been deleted (it won't be in the geofences list)
    //    var projectGeofenceAssociations = (await projectRepo.GetAssociatedGeofences(projectUid).ConfigureAwait(false))
    //      .Where(pg => !geofenceTypes.Any() || geofenceTypes.Contains((int)pg.GeofenceType));
    //    var geofencesAssociated = geofences
    //      .Where(g => projectGeofenceAssociations.Any(pg => pg.GeofenceUID == g.GeofenceUID));

    //    var associated = geofencesAssociated.ToList();
    //    log.LogInformation(
    //      $"Geofence list contains {associated.Count()} geofences associated to project {projectUid}");
    //    return associated.ToList();
    //  }

    //  var enumerable = geofences.ToList();
    //  log.LogInformation($"Geofence list contains {enumerable.Count()} available geofences");
    //  return enumerable.ToList();
    //}

    #region rollback

    /// <summary>
    /// Used internally, if a step fails, after a project has been CREATED, 
    ///    then delete it permanently i.e. don't just set IsDeleted.
    /// Since v4 CreateProjectInDB also associates projectCustomer then roll this back also.
    /// DissociateProjectCustomer actually deletes the DB ent4ry
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    /// <param name="log"></param>
    /// <param name="projectRepo"></param>
    /// <returns></returns>
    public static async Task DeleteProjectPermanentlyInDb(Guid customerUid, Guid projectUid, ILogger log, IProjectRepository projectRepo)
    {
      log.LogDebug($"DeleteProjectPermanentlyInDB: {projectUid}");
      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = projectUid,
        DeletePermanently = true,
        ActionUTC = DateTime.UtcNow
      };
      await projectRepo.StoreEvent(deleteProjectEvent).ConfigureAwait(false);

      await projectRepo.StoreEvent(new DissociateProjectCustomer
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ActionUTC = DateTime.UtcNow
      }).ConfigureAwait(false);
    }

    #endregion rollback

  }
}
