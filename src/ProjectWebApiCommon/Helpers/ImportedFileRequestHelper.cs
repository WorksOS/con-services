using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public class ImportedFileRequestHelper
  {

    /// <summary>
    /// Gets the imported file list for a project
    /// </summary>
    /// <returns></returns>
    public static async Task<ImmutableList<ImportedFile>> GetImportedFiles(string projectUid, ILogger log, IProjectRepository projectRepo)
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
    public static async Task<ImmutableList<ImportedFileDescriptor>> GetImportedFileList(string projectUid, ILogger log, string userId, IProjectRepository projectRepo)
    {
      var importedFiles = await GetImportedFiles(projectUid, log, projectRepo).ConfigureAwait(false);

      var importedFileList = importedFiles.Select(importedFile =>
          AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>(importedFile))
        .ToList();

      var deactivatedFileList = await GetImportedFileProjectSettings(projectUid, userId, projectRepo).ConfigureAwait(false);
      if (deactivatedFileList != null)
      {
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

    public static async Task<List<ActivatedFileDescriptor>> GetImportedFileProjectSettings(string projectUid, string userId, IProjectRepository projectRepo)
    {
      List<ActivatedFileDescriptor> deactivatedFileList = null;
      var importFileSettings = await projectRepo.GetProjectSettings(projectUid, userId, ProjectSettingsType.ImportedFiles).ConfigureAwait(false);
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
        createImportedFileEvent.ImportedFileID = existing.ImportedFileId;
      else
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 50);
      }

      log.LogDebug(
        $"CreateImportedFileinDb: Legacy importedFileId {createImportedFileEvent.ImportedFileID} for ImportedFile {filename} for project {projectUid}.");
      return createImportedFileEvent;
    }

    /// <summary>
    /// Notify raptor of new file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    public static async Task<AddFileResult> NotifyRaptorAddFile(long? projectId, Guid projectUid, ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, FileDescriptor fileDescriptor, long importedFileId, Guid importedFileUid, bool isCreate,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler, IRaptorProxy raptorProxy, IProjectRepository projectRepo )
    {
      AddFileResult notificationResult = null;
      try
      {
        notificationResult = await raptorProxy
          .AddFile(projectUid, importedFileType, importedFileUid,
            JsonConvert.SerializeObject(fileDescriptor), importedFileId, dxfUnitsType, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"FileImport AddFile in RaptorServices failed with exception. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. isCreate: {isCreate}. Exception Thrown: {e.Message}. ");
        if (isCreate)
          await DeleteImportedFileInDb(projectUid, importedFileUid, serviceExceptionHandler, projectRepo, true).ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.AddFile", e.Message);
      }
      log.LogDebug(
        $"NotifyRaptorAddFile: projectId: {projectId} projectUid: {projectUid}, FileDescriptor: {JsonConvert.SerializeObject(fileDescriptor)}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        log.LogError($"FileImport AddFile in RaptorServices failed. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"} isCreate: {isCreate}. ");
        if (isCreate)
          await DeleteImportedFileInDb(projectUid, importedFileUid, serviceExceptionHandler, projectRepo, true).ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67, notificationResult.Code.ToString(), notificationResult.Message);
      }
      return notificationResult;
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
    /// Copies importedFile between filespaces in TCC
    ///     From FilespaceIDBcCustomer\BC Data to FilespaceIdVisionLink\CustomerUid\ProjectUid
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists
    /// </summary>
    /// <returns></returns>
    public static async Task<FileDescriptor> CopyFileWithinTccRepository(BusinessCenterFile sourceFile,
      string customerUid, string projectUid, string fileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      // todo see if there is a TCC command to CopyFile BETWEEN filespaceIDs?
      Stream memStream = null;
      var tccPathSource = $"{sourceFile.Path}/{sourceFile.Name}";

      var tccPathTarget = $"/{customerUid}/{projectUid}";
      var tccFileNameTarget = sourceFile.Name;

      try
      {
        /*
         // todo change mock or MOQ to return true
         MockFileRepository.FolderExists and FileExists always returns false so can't check first
        // check for exists first to avoid an misleading exception in our logs.
        var folderExists = await fileRepo.FolderExists(businessCentreFile.FileSpaceId, tccPath).ConfigureAwait(false);
        if (!folderExists)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 78,
            $"{businessCentreFile.FileSpaceId} {tccPath}");
        }
        */

        log.LogInformation(
          $"CopyFileWithinTccRepository: getFile filespaceID: {sourceFile.FileSpaceId} tccPathSource: {tccPathSource}");
        memStream = await fileRepo.GetFile(sourceFile.FileSpaceId, tccPathSource).ConfigureAwait(false);

        if (memStream != null && memStream.CanRead && memStream.Length > 0)
        {
          // note that the filename already contains the surveyUtc where appropriate

          bool ccPutFileResult = false;
          try
          {
            log.LogInformation(
              $"CopyFileWithinTccRepository: fileSpaceId {fileSpaceId} tccPathTarget {tccPathTarget} tccFileNameTarget {tccFileNameTarget}");
            // check for exists first to avoid an misleading exception in our logs.
            var folderAlreadyExists = await fileRepo.FolderExists(fileSpaceId, tccPathTarget).ConfigureAwait(false);
            if (folderAlreadyExists == false)
              await fileRepo.MakeFolder(fileSpaceId, tccPathTarget).ConfigureAwait(false);

            // this does an upsert
            ccPutFileResult = await fileRepo.PutFile(fileSpaceId, tccPathTarget, tccFileNameTarget, memStream, memStream.Length)
              .ConfigureAwait(false);
          }
          catch (Exception e)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "fileRepo.PutFile",
              e.Message);
          }
          finally
          {
            memStream.Dispose();
          }


          if (ccPutFileResult == false)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 53);
          }
        }
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
            80, $" isAbleToRead {memStream != null && memStream.CanRead} bytesReturned: {memStream?.Length ?? 0}");
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

      var fileDescriptorTarget = FileDescriptor.CreateFileDescriptor(fileSpaceId, tccPathTarget, tccFileNameTarget);
      log.LogInformation(
        $"CopyFileWithinTccRepository: fileDescriptorTarget {JsonConvert.SerializeObject(fileDescriptorTarget)}");
      return fileDescriptorTarget;
    }
  }
}
