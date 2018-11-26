using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Models.Models;
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

    /// <summary>
    /// Notify raptor of delete file
    ///  if it doesn't know about it then it do nothing and return success
    /// </summary>
    /// <returns></returns>
    public static async Task<ImportedFileInternalResult> NotifyRaptorDeleteFile(Guid projectUid, ImportedFileType importedFileType,
      Guid importedFileUid, FileDescriptor fileDescriptor, long importedFileId, long? legacyImportedFileId,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler,
      IProjectRepository projectRepo, IRaptorProxy raptorProxy)
    {
      BaseDataResult notificationResult = null;
      try
      {
        notificationResult = await raptorProxy
          .DeleteFile(projectUid, importedFileType, importedFileUid, JsonConvert.SerializeObject(fileDescriptor), importedFileId, legacyImportedFileId, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"NotifyRaptorDeleteFile DeleteFile in RaptorServices failed with exception. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Exception Thrown: {e.Message}.");
        return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 57, "raptorProxy.DeleteFile", e.Message);
      }

      log.LogDebug(
            $"FileImport DeleteFile in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");
      if (notificationResult != null && notificationResult.Code != 0)
      {
        log.LogError($"NotifyRaptorDeleteFile DeleteFile in RaptorServices failed. projectUid:{projectUid} FileDescriptor:{fileDescriptor}. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}");
        return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 108, notificationResult.Code.ToString(), notificationResult.Message);
      }
      return null;
    }
    #endregion raptor

    #region TRex

    /// <summary>
    /// Notify TRex of new DESIGN file
    /// </summary>
    /// <returns></returns>
    public static async Task<ContractExecutionResult> NotifyTRexAddFile(Guid projectUid,
      ImportedFileType importedFileType, string filename, Guid importedFileUid, DateTime? surveyedUtc,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler,
      ITRexImportFileProxy tRexImportFileProxy, IProjectRepository projectRepo
    )
    {
      var result = new ContractExecutionResult();

      string fullFileName = filename;
      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc != null)
        fullFileName =
          ImportedFileUtils.IncludeSurveyedUtcInName(fullFileName, surveyedUtc.Value);
      var request = new DesignRequest(projectUid, importedFileType, fullFileName, importedFileUid, surveyedUtc);
      try
      {
        result = await tRexImportFileProxy
          .AddFile(request, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"NotifyTRexAddFile AddFile in Trex gateway failed with exception. request:{JsonConvert.SerializeObject(request)} filename: {fullFileName} exception: {e.Message}. ");

        await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb
          (projectUid, importedFileUid, serviceExceptionHandler, projectRepo, true)
            .ConfigureAwait(false);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "tRexImportFile.AddFile",
          e.Message);
      }

      log.LogDebug(
        $"NotifyTRexAddFile: projectUid: {projectUid}, filename: {filename} importedFileUid {importedFileUid}. TRex returned code: {result?.Code ?? -1}.");

      if (result != null && result.Code != 0)
      {
        log.LogError(
          $"NotifyTRexAddFile AddFile in Trex failed. projectUid: {projectUid}, filename: {filename} importedFileUid {importedFileUid}. Reason: {result?.Code ?? -1} {result?.Message ?? "null"}.");
        
        await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb(projectUid, importedFileUid,
              serviceExceptionHandler, projectRepo, true)
            .ConfigureAwait(false);

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 114,
          result.Code.ToString(), result.Message);
      }

      return result;
    }

    /// <summary>
    /// Notify TRex of updated DESIGN file
    /// </summary>
    public static async Task<ContractExecutionResult> NotifyTRexUpdateFile(Guid projectUid,
      ImportedFileType importedFileType, string filename, Guid importedFileUid, DateTime? surveyedUtc,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler,
      ITRexImportFileProxy tRexImportFileProxy, IProjectRepository projectRepo
    )
    {
      var result = new ContractExecutionResult();
      string fullFileName = filename;
      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc != null)
        fullFileName =
          ImportedFileUtils.IncludeSurveyedUtcInName(fullFileName, surveyedUtc.Value);
      var request = new DesignRequest(projectUid, importedFileType, fullFileName, importedFileUid, surveyedUtc);
      try
      {
        result = await tRexImportFileProxy
          .UpdateFile(request, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"NotifyTRexAddFile UpdateFile in Trex gateway failed with exception. request:{JsonConvert.SerializeObject(request)} filename: {fullFileName} exception: {e.Message}. ");

        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "tRexImportFile.UpdateFile",
          e.Message);
      }

      return result;
    }

    /// <summary>
    /// Notify trex of delete of a design file
    /// </summary>
    /// <returns></returns>
    public static async Task<ContractExecutionResult> NotifyTRexDeleteFile(Guid projectUid,
      ImportedFileType importedFileType, string filename, Guid importedFileUid, DateTime? surveyedUtc,
      ILogger log, IDictionary<string, string> headers, IServiceExceptionHandler serviceExceptionHandler,
      ITRexImportFileProxy tRexImportFileProxy, IProjectRepository projectRepo
    )
    {
      var result = new ContractExecutionResult();
      string fullFileName = filename;
      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc != null)
        fullFileName =
          ImportedFileUtils.IncludeSurveyedUtcInName(fullFileName, surveyedUtc.Value);
      var request = new DesignRequest(projectUid, importedFileType, fullFileName, importedFileUid, surveyedUtc);
      try
      {
        result = await tRexImportFileProxy
          .DeleteFile(request, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(
          $"NotifyTRexAddFile DeleteFile in Trex gateway failed with exception. request:{JsonConvert.SerializeObject(request)} filename: {fullFileName} exception: {e.Message}. ");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "tRexImporFile.DeleteFile",
          e.Message);
      }

      return result;
    }

    #endregion TRex
  }

}
