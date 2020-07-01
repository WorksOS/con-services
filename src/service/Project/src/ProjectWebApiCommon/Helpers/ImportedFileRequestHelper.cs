using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Extensions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public static class ImportedFileRequestHelper
  {
    #region TRex

    /// <summary>
    /// Notify TRex of new DESIGN file
    /// </summary>
    /// <returns></returns>
    public static async Task<ContractExecutionResult> NotifyTRexAddFile(Guid projectUid,
      ImportedFileType importedFileType, string filename, Guid importedFileUid, DateTime? surveyedUtc,
      ILogger log, IHeaderDictionary headers, IServiceExceptionHandler serviceExceptionHandler,
      ITRexImportFileProxy tRexImportFileProxy, IProjectRepository projectRepo
    )
    {
      var result = new ContractExecutionResult();

      string fullFileName = filename;
      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc != null)
        fullFileName =
          fullFileName.IncludeSurveyedUtcInName(surveyedUtc.Value);
      var request = new DesignRequest(projectUid, importedFileType, fullFileName, importedFileUid, surveyedUtc);
      try
      {
        result = await tRexImportFileProxy
          .AddFile(request, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(e, $"NotifyTRexAddFile AddFile in Trex gateway failed with exception. request:{JsonConvert.SerializeObject(request)} filename: {fullFileName}");

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
      ILogger log, IHeaderDictionary headers, IServiceExceptionHandler serviceExceptionHandler,
      ITRexImportFileProxy tRexImportFileProxy)
    {
      var result = new ContractExecutionResult();
      string fullFileName = filename;
      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc != null)
        fullFileName =
          fullFileName.IncludeSurveyedUtcInName(surveyedUtc.Value);
      var request = new DesignRequest(projectUid, importedFileType, fullFileName, importedFileUid, surveyedUtc);
      try
      {
        result = await tRexImportFileProxy
          .UpdateFile(request, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(e, $"NotifyTRexAddFile UpdateFile in Trex gateway failed with exception. request:{JsonConvert.SerializeObject(request)} filename: {fullFileName}");

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
      ILogger log, IHeaderDictionary headers, IServiceExceptionHandler serviceExceptionHandler,
      ITRexImportFileProxy tRexImportFileProxy)
    {
      var result = new ContractExecutionResult();
      string fullFileName = filename;
      if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc != null)
        fullFileName =
          fullFileName.IncludeSurveyedUtcInName(surveyedUtc.Value);
      var request = new DesignRequest(projectUid, importedFileType, fullFileName, importedFileUid, surveyedUtc);
      try
      {
        result = await tRexImportFileProxy
          .DeleteFile(request, headers)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        log.LogError(e, $"NotifyTRexAddFile DeleteFile in Trex gateway failed with exception. request:{JsonConvert.SerializeObject(request)} filename: {fullFileName}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "tRexImporFile.DeleteFile",
          e.Message);
      }

      return result;
    }

    #endregion TRex

  }
}
