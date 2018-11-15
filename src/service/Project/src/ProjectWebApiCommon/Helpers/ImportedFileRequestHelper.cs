using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public class ImportedFileRequestHelper
  {
    #region raptor

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
          await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb(projectUid, importedFileUid,
              serviceExceptionHandler, projectRepo, true)
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
          await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb(projectUid, importedFileUid,
              serviceExceptionHandler, projectRepo, true)
            .ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67,
          notificationResult.Code.ToString(), notificationResult.Message);
      }

      return notificationResult;
    }

    #endregion raptor

    #region TRex

    /// <summary>
    /// Notify TRex of new DESIGN file
    /// </summary>
    /// <returns></returns>
    public static async Task<ContractExecutionResult> NotifyTRexAddFile(Guid projectUid,
      ImportedFileType importedFileType, string filename, Guid importedFileUid, DateTime? surveyedUtc,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler
      // IRaptorProxy raptorProxy, IProjectRepository projectRepo
    )
    {
      var notificationResult = new ContractExecutionResult();

      //  todoJeannie
      //try
      //{
      //  notificationResult = await raptorProxy
      //    .AddFile(projectUid, importedFileType, importedFileUid,
      //      JsonConvert.SerializeObject(fileDescriptor), importedFileId, dxfUnitsType, headers)
      //    .ConfigureAwait(false);
      //}
      //catch (Exception e)
      //{
      //  log.LogError(
      //    $"FileImport AddFile in RaptorServices failed with exception. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. isCreate: {isCreate}. Exception Thrown: {e.Message}. ");
      //  if (isCreate)
      //    await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb(projectUid, importedFileUid, serviceExceptionHandler, projectRepo, true)
      //      .ConfigureAwait(false);
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.AddFile",
      //    e.Message);
      //}

      //log.LogDebug(
      //  $"NotifyRaptorAddFile: projectId: {projectId} projectUid: {projectUid}, FileDescriptor: {JsonConvert.SerializeObject(fileDescriptor)}. RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      //if (notificationResult != null && notificationResult.Code != 0)
      //{
      //  log.LogError(
      //    $"FileImport AddFile in RaptorServices failed. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"} isCreate: {isCreate}. ");
      //  if (isCreate)
      //    await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb(projectUid, importedFileUid, serviceExceptionHandler, projectRepo, true)
      //      .ConfigureAwait(false);

      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 67,
      //    notificationResult.Code.ToString(), notificationResult.Message);
      //}

      return notificationResult;
    }

    /// <summary>
    /// Notify raptor of an updated import file.
    /// </summary>
    public static async Task NotifyTRexUpdateFile(Guid projectUid,
      ImportedFileType importedFileType, string filename, Guid importedFileUid, DateTime? surveyedUtc,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler
      // IRaptorProxy raptorProxy, IProjectRepository projectRepo
    )
    {
      // todoJeannie
      var notificationResult = new ContractExecutionResult();
      //log.LogDebug(
      //  $"FileImport UpdateFiles in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      //if (notificationResult != null && notificationResult.Code != 0)
      //{
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 108, notificationResult.Code.ToString(), notificationResult.Message);
      //}
    }

    /// <summary>
    /// Notify trex of delete file
    /// </summary>
    /// <returns></returns>
    public static async Task NotifyTRexDeleteFile(Guid projectUid, 
      ImportedFileType importedFileType, Guid importedFileUid, string fileName,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler
      )
    {
      // todoJeannie
      //BaseDataResult notificationResult = null;
      //try
      //{
      //  notificationResult = await raptorProxy
      //    .DeleteFile(projectUid, importedFileType, importedFileUid, fileDescriptor, importedFileId, legacyImportedFileId, Request.Headers.GetCustomHeaders())
      //    .ConfigureAwait(false);
      //}
      //catch (Exception e)
      //{
      //  log.LogError(
      //    $"FileImport DeleteFile in RaptorServices failed with exception. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Exception Thrown: {e.Message}.");

      //  await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "raptorProxy.DeleteFile", e.Message);
      //}

      //log.LogDebug(
      //  $"FileImport DeleteFile in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      //if (notificationResult != null && notificationResult.Code != 0)
      //{
      //  log.LogError($"FileImport DeleteFile in RaptorServices failed. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}");

      //  await UndeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);
      //  serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 108, notificationResult.Code.ToString(), notificationResult.Message);
      //}
    }

    #endregion TRex
  }
}
