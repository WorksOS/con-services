using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public class ImportedFileRequestDatabaseHelper
  {
    /// <summary>
    /// Gets the imported file list for a project
    /// </summary>
    /// <returns></returns>
    public static async Task<ImmutableList<ImportedFile>> GetImportedFiles(string projectUid, ILogger log,
      IProjectRepository projectRepo)
    {
      var importedFiles = (await projectRepo.GetImportedFiles(projectUid).ConfigureAwait(false))
        .ToImmutableList();

      log.LogInformation($"ImportedFile list contains {importedFiles.Count} importedFiles");
      return importedFiles;
    }

    /// <summary>
    /// Gets the imported file list for a project in Response
    /// </summary>
    /// <returns></returns>
    public static async Task<ImmutableList<ImportedFileDescriptor>> GetImportedFileList(string projectUid, ILogger log,
      string userId, IProjectRepository projectRepo)
    {
      var importedFiles = await GetImportedFiles(projectUid, log, projectRepo).ConfigureAwait(false);
      log.LogInformation($"GetImportedFileList importedFilesList contains {importedFiles.Count} importedFiles");

      var importedFileList = importedFiles.Select(importedFile =>
          AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>(importedFile))
        .ToList();

      var deactivatedFileList =
        await GetImportedFileProjectSettings(projectUid, userId, projectRepo).ConfigureAwait(false);
      if (deactivatedFileList != null)
      {
        log.LogInformation(
          $"GetImportedFileList deactivatedFileList contains {deactivatedFileList.Count} importedFiles");
        foreach (var activatedFileDescr in deactivatedFileList)
        {
          var importedFile =
            importedFileList.SingleOrDefault(i => i.ImportedFileUid == activatedFileDescr.ImportedFileUid);
          if (importedFile != null)
          {
            importedFile.IsActivated = activatedFileDescr.IsActivated;
          }
        }
      }

      return importedFileList.ToImmutableList();
    }

    public static async Task<ImportedFile> GetImportedFileForProject
      (string projectUid, string fileName, ImportedFileType importedFileType, DateTime? surveyedUtc,
      ILogger log, IProjectRepository projectRepo)
    {
      var importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFiles(projectUid, log, projectRepo).ConfigureAwait(false);
      ImportedFile existing = null;
      if (importedFiles.Count > 0)
      {
        existing = importedFiles.FirstOrDefault(
          f => string.Equals(f.Name, fileName, StringComparison.OrdinalIgnoreCase)
               && f.ImportedFileType == importedFileType
               && (
                 importedFileType == ImportedFileType.SurveyedSurface &&
                 f.SurveyedUtc == surveyedUtc ||
                 importedFileType != ImportedFileType.SurveyedSurface
               ));
      }
      return existing;
    }

  public static async Task<List<ActivatedFileDescriptor>> GetImportedFileProjectSettings(string projectUid,
      string userId, IProjectRepository projectRepo)
    {
      List<ActivatedFileDescriptor> deactivatedFileList = null;
      var importFileSettings = await projectRepo
        .GetProjectSettings(projectUid, userId, ProjectSettingsType.ImportedFiles).ConfigureAwait(false);
      if (importFileSettings != null)
      {
        deactivatedFileList = JsonConvert.DeserializeObject<List<ActivatedFileDescriptor>>(importFileSettings.Settings);
      }

      return deactivatedFileList;
    }

    /// <summary>
    /// Creates an imported file in Db.
    /// </summary>
    /// <returns />
    public static async Task<CreateImportedFileEvent> CreateImportedFileinDb(Guid customerUid, Guid projectUid,
      ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, string filename, DateTime? surveyedUtc,
      string fileDescriptor, DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      log.LogDebug($"Creating the ImportedFile {filename} for project {projectUid}.");
      var nowUtc = DateTime.UtcNow;
      var createImportedFileEvent = new CreateImportedFileEvent
      {
        CustomerUID = customerUid,
        ProjectUID = projectUid,
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileType = importedFileType,
        DxfUnitsType = dxfUnitsType,
        Name = filename,
        FileDescriptor = fileDescriptor,
        FileCreatedUtc = fileCreatedUtc,
        FileUpdatedUtc = fileUpdatedUtc,
        ImportedBy = importedBy,
        SurveyedUTC = surveyedUtc,
        ActionUTC = nowUtc, // aka importedUtc
        ReceivedUTC = nowUtc
      };

      var isCreated = await projectRepo.StoreEvent(createImportedFileEvent).ConfigureAwait(false);
      if (isCreated == 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 49);
      }

      log.LogDebug($"Created the ImportedFile in DB. ImportedFile {filename} for project {projectUid}.");

      // plug the legacyID back into the struct to be injected into kafka
      var existing = await projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
        .ConfigureAwait(false);
      if (existing != null && existing.ImportedFileId > 0)
      {
        createImportedFileEvent.ImportedFileID = existing.ImportedFileId;
        createImportedFileEvent.ImportedFileUID = Guid.Parse(existing.ImportedFileUid); // for unit tests
      }
      else
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 50);
      }

      log.LogDebug(
        $"CreateImportedFileinDb: Legacy importedFileId {createImportedFileEvent.ImportedFileID} for ImportedFile {filename} for project {projectUid}.");
      return createImportedFileEvent;
    }

    /// <summary>
    /// Deletes imported file from the Db.
    /// </summary>
    /// <returns />
    public static async Task<DeleteImportedFileEvent> DeleteImportedFileInDb(Guid projectUid, Guid importedFileUid,
      IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo, bool deletePermanently = false)
    {
      var nowUtc = DateTime.UtcNow;
      var deleteImportedFileEvent = new DeleteImportedFileEvent
      {
        ProjectUID = projectUid,
        ImportedFileUID = importedFileUid,
        DeletePermanently = deletePermanently,
        ActionUTC = nowUtc, // aka importedDate
        ReceivedUTC = nowUtc
      };

      if (await projectRepo.StoreEvent(deleteImportedFileEvent).ConfigureAwait(false) == 1)
        return deleteImportedFileEvent;

      serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 51);
      return deleteImportedFileEvent;
    }

    /// <summary>
    /// Update an imported file in the Db.
    /// </summary>
    /// <param name="existing">The existing imported file event from the database</param>
    /// <param name="fileDescriptor"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="minZoom"></param>
    /// <param name="maxZoom"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="importedBy"></param>
    /// <param name="log"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="projectRepo"></param>
    /// <returns></returns>
    public static async Task<UpdateImportedFileEvent> UpdateImportedFileInDb(
      ImportedFile existing,
      string fileDescriptor, DateTime? surveyedUtc, int minZoom, int maxZoom,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, string importedBy,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var nowUtc = DateTime.UtcNow;
      var updateImportedFileEvent = AutoMapperUtility.Automapper.Map<UpdateImportedFileEvent>(existing);
      updateImportedFileEvent.FileDescriptor = fileDescriptor;
      updateImportedFileEvent.SurveyedUtc = surveyedUtc;
      updateImportedFileEvent.MinZoomLevel = minZoom;
      updateImportedFileEvent.MaxZoomLevel = maxZoom;
      updateImportedFileEvent.FileCreatedUtc = fileCreatedUtc; // as per Barret 19th June 2017
      updateImportedFileEvent.FileUpdatedUtc = fileUpdatedUtc;
      updateImportedFileEvent.ImportedBy = importedBy;
      updateImportedFileEvent.ActionUTC = nowUtc;
      updateImportedFileEvent.ReceivedUTC = nowUtc;

      log.LogInformation(
        $"UpdateImportedFileInDb. UpdateImportedFileEvent: {JsonConvert.SerializeObject(updateImportedFileEvent)}");

      if (await projectRepo.StoreEvent(updateImportedFileEvent).ConfigureAwait(false) == 1)
        return updateImportedFileEvent;

      serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 52);
      return updateImportedFileEvent;
    }

    /// <summary>
    /// un-deletes imported file from the Db using the Respositories library.
    /// Used solely for rollback and is never inserted in the kafka que.
    /// </summary>
    /// <returns />
    public static async Task UndeleteImportedFile(Guid projectUid, Guid importedFileUid,
      IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo)
    {
      var nowUtc = DateTime.UtcNow;
      var undeleteImportedFileEvent = new UndeleteImportedFileEvent
      {
        ProjectUID = projectUid,
        ImportedFileUID = importedFileUid,
        ActionUTC = nowUtc,
        ReceivedUTC = nowUtc
      };

      if (await projectRepo.StoreEvent(undeleteImportedFileEvent).ConfigureAwait(false) == 1)
        return;

      serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 51);
    }


    /// <summary>
    /// Get the list of filters for the project
    /// </summary>
    public static async Task<List<VSS.MasterData.Models.Models.Filter>> GetFilters(Guid projectUid, 
      IDictionary<string, string> customHeaders, IFilterServiceProxy filterServiceProxy)
    {
      var filterDescriptors = await filterServiceProxy.GetFilters(projectUid.ToString(), customHeaders);
      if (filterDescriptors == null || filterDescriptors.Count == 0)
      {
        return null;
      }

      return filterDescriptors.Select(f => JsonConvert.DeserializeObject<VSS.MasterData.Models.Models.Filter>(f.FilterJson)).ToList();
    }
  }
}
