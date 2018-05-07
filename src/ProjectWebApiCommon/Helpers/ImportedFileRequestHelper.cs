using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.TCCFileAccess.Models;
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
        log.LogInformation($"GetImportedFileList deactivatedFileList contains {deactivatedFileList.Count} importedFiles");
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
    public static async Task<AddFileResult> NotifyRaptorAddFile(long? projectId, Guid projectUid,
      ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, FileDescriptor fileDescriptor, long importedFileId,
      Guid importedFileUid, bool isCreate,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler,
      IRaptorProxy raptorProxy, IProjectRepository projectRepo)
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
          await DeleteImportedFileInDb(projectUid, importedFileUid, serviceExceptionHandler, projectRepo, true)
            .ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.AddFile",
          e.Message);
      }

      log.LogDebug(
        $"NotifyRaptorAddFile: projectId: {projectId} projectUid: {projectUid}, FileDescriptor: {JsonConvert.SerializeObject(fileDescriptor)}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        log.LogError(
          $"FileImport AddFile in RaptorServices failed. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"} isCreate: {isCreate}. ");
        if (isCreate)
          await DeleteImportedFileInDb(projectUid, importedFileUid, serviceExceptionHandler, projectRepo, true)
            .ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67,
          notificationResult.Code.ToString(), notificationResult.Message);
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
    /// Get the FileCreated and Updated UTCs
    ///    and checks that the file exists.
    /// </summary>
    /// <returns></returns>
    public static async Task<DirResult> GetFileInfoFromTccRepository(BusinessCenterFile sourceFile,
      string fileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      DirResult fileEntry = null;

      try
      {
        log.LogInformation(
          $"CopyFileWithinTccRepository: GetFileList filespaceID: {sourceFile.FileSpaceId} tccPathSource: {sourceFile.Path} sourceFile.Name: {sourceFile.Name}");

        var dirResult = await fileRepo.GetFileList(sourceFile.FileSpaceId, sourceFile.Path, sourceFile.Name);

        log.LogInformation(
          $"CopyFileWithinTccRepository: GetFileList dirResult: {JsonConvert.SerializeObject(dirResult)}");


        if (dirResult == null || dirResult.entries.Length == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94, "fileRepo.GetFileList");
        }
        else
        {
          fileEntry = dirResult.entries.FirstOrDefault(f =>
            !f.isFolder && (string.Compare(f.entryName, sourceFile.Name, true, CultureInfo.InvariantCulture) == 0));
          if (fileEntry == null)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94,
              "fileRepo.GetFileList");
          }
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 94, "fileRepo.GetFileList",
          e.Message);
      }

      return fileEntry;
    }

    /// <summary>
    /// Copies importedFile between filespaces in TCC
    ///     From FilespaceIDBcCustomer\BC Data to FilespaceIdVisionLink\CustomerUid\ProjectUid
    ///   returns filespaceID; path and filename which identifies it uniquely in TCC
    ///   this may be a create or update, so ok if it already exists
    /// </summary>
    /// <returns></returns>
    public static async Task<FileDescriptor> CopyFileWithinTccRepository(BusinessCenterFile sourceFile,
      string customerUid, string projectUid, string dstFileSpaceId,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo)
    {
      var srcTccPathAndFile = $"{sourceFile.Path}/{sourceFile.Name}";
      var destTccPath = $"/{customerUid}/{projectUid}";
      var destTccPathAndFile = $"/{customerUid}/{projectUid}/{sourceFile.Name}";
      var tccCopyFileResult = false;

      try
      {
        // The filename already contains the surveyUtc where appropriate
        log.LogInformation(
          $"CopyFileWithinTccRepository: srcFileSpaceId: {sourceFile.FileSpaceId} destFileSpaceId {dstFileSpaceId} srcTccPathAndFile {srcTccPathAndFile} destTccPathAndFile {destTccPathAndFile}");

        // check for exists first to avoid an misleading exception in our logs.
        var folderAlreadyExists = await fileRepo.FolderExists(dstFileSpaceId, destTccPath).ConfigureAwait(false);
        if (folderAlreadyExists == false)
          await fileRepo.MakeFolder(dstFileSpaceId, destTccPath).ConfigureAwait(false);

        // this creates folder if it doesn't exist, and upserts file if it does
        tccCopyFileResult = await fileRepo
          .CopyFile(sourceFile.FileSpaceId, dstFileSpaceId, srcTccPathAndFile, destTccPathAndFile)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 92, "fileRepo.PutFile",
          e.Message);
      }

      if (tccCopyFileResult == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 92);
      }

      var fileDescriptorTarget = FileDescriptor.CreateFileDescriptor(dstFileSpaceId, destTccPath, sourceFile.Name);
      log.LogInformation(
        $"CopyFileWithinTccRepository: fileDescriptorTarget {JsonConvert.SerializeObject(fileDescriptorTarget)}");
      return fileDescriptorTarget;
    }
  }
}
