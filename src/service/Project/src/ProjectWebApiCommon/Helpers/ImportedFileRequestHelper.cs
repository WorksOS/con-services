using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Jaeger.Thrift;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

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
            JsonConvert.SerializeObject(fileDescriptor), importedFileId, dxfUnitsType, headers);

      
      }
      catch (Exception e)
      {
        log.LogError(e, $"FileImport AddFile in RaptorServices failed with exception. projectId:{projectId} projectUid:{projectUid} FileDescriptor:{fileDescriptor}. isCreate: {isCreate}");
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
        log.LogError(e, $"NotifyRaptorDeleteFile DeleteFile in RaptorServices failed with exception. projectUid:{projectUid} FileDescriptor:{fileDescriptor}");
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
        log.LogError(e, $"NotifyTRexAddFile DeleteFile in Trex gateway failed with exception. request:{JsonConvert.SerializeObject(request)} filename: {fullFileName}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "tRexImporFile.DeleteFile",
          e.Message);
      }

      return result;
    }

    #endregion TRex

    #region tiles

    /// <summary>
    /// Generate DXF tiles using Pegasus through the tile service. Set the zoom range in the notification result.
    /// </summary>
    public static async Task GenerateDxfTiles(AddFileResult notificationResult, string rootFolder, Guid projectUid, string customerUid, 
      string fileName, ImportedFileType importedFileType, DxfUnitsType dxfUnitsType, string coordSysFileName, Guid importedFileUid, 
      ILogger log, IDictionary<string, string> headers, ITileServiceProxy tileServiceProxy, IRaptorProxy raptorProxy, 
      IServiceExceptionHandler serviceExceptionHandler, ITPaaSApplicationAuthentication authn, IDataOceanClient dataOceanClient, 
      IConfigurationStore configStore)
    { 
      if (importedFileType == ImportedFileType.Linework || importedFileType == ImportedFileType.Alignment)
      {
        try
        {
          //For alignment files get the generated DXF from Raptor and save to DataOcean
          if (importedFileType == ImportedFileType.Alignment)
          {
            fileName = await CreateGeneratedDxfFile(customerUid, projectUid, importedFileUid, raptorProxy, headers, 
              log, serviceExceptionHandler, authn, dataOceanClient, configStore);
          }

          var dataOceanPath = DataOceanHelper.DataOceanPath(rootFolder, customerUid, projectUid.ToString());
          var dxfFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{fileName}";
          var dcFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{coordSysFileName}";
          const string PEGASUS_EXECUTION_TIMEOUT_KEY = "PEGASUS_EXECUTION_TIMEOUT_MINS";
          var executionTimeout = configStore.GetValueInt(PEGASUS_EXECUTION_TIMEOUT_KEY, 5)*60000;//minutes converted to millisecs
          //TODO: If this takes a very long time we need to implement a notification for the client when it is done.
          var tileMetadata = await tileServiceProxy.GenerateDxfTiles(dcFileName, dxfFileName, dxfUnitsType, headers, executionTimeout);
          if (tileMetadata != null)
          {
            notificationResult.MinZoomLevel = tileMetadata.MinZoom;
            notificationResult.MaxZoomLevel = tileMetadata.MaxZoom;
          }
        }
        catch (Exception e)
        {
          log.LogError(
            $"FileImport GenerateDxfTiles in TileService failed with exception. projectUid:{projectUid} fileName:{fileName}. Exception Thrown: {e.Message}. ");
          throw;
        }        
      }
    }

    /// <summary>
    /// Delete generated DXF tiles through the tile service.
    /// </summary>
    public static async Task DeleteDxfTiles(string rootFolder, Guid projectUid, string customerUid, string fileName, 
      ImportedFileType importedFileType, ILogger log, IDictionary<string, string> headers, ITileServiceProxy tileServiceProxy)
    {
      if (importedFileType == ImportedFileType.Linework || importedFileType == ImportedFileType.Alignment)
      {
        try
        {
          var dataOceanPath = DataOceanHelper.DataOceanPath(rootFolder, customerUid, projectUid.ToString());
          var dxfFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{fileName}";
          var success = await tileServiceProxy.DeleteDxfTiles(dxfFileName, headers);
        }
        catch (Exception e)
        {
          log.LogError(
            $"FileImport DeleteDxfTiles in TileService failed with exception. projectUid:{projectUid} fileName:{fileName}. Exception Thrown: {e.Message}. ");
          throw;
        }
      }
    }

    /// <summary>
    /// Create a DXF file of the alignment center line using Raptor and save it to data ocean.
    /// </summary>
    /// <returns>The generated file name</returns>
    private static async Task<string> CreateGeneratedDxfFile(string customerUid, Guid projectUid, Guid alignmentUid, IRaptorProxy raptorProxy, IDictionary<string, string> headers,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, ITPaaSApplicationAuthentication authn, IDataOceanClient dataOceanClient, IConfigurationStore configStore)
    {
      var generatedName = string.Empty;
      //Get generated DXF file from Raptor
      var dxfContents = await raptorProxy.GetLineworkFromAlignment(projectUid, alignmentUid, headers);
      //GradefulWebRequest should throw an exception if the web api call fails but just in case...
      if (dxfContents != null && dxfContents.Length > 0)
      {
        //Unzip it and save to DataOcean 
        using (var archive = new ZipArchive(dxfContents))
        {
          if (archive.Entries.Count == 1)
          {
            generatedName = archive.Entries[0].Name;
            var dataOceanRootFolder = configStore.GetValueString("DATA_OCEAN_ROOT_FOLDER");
            if (string.IsNullOrEmpty(dataOceanRootFolder))
            {
              serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 115);
            }
            using (var stream = archive.Entries[0].Open() as DeflateStream)
            using (var ms = new MemoryStream())
            {
              // Unzip the file, copy to memory 
              stream.CopyTo(ms);
              ms.Seek(0, SeekOrigin.Begin);
              await DataOceanHelper.WriteFileToDataOcean(
                ms, dataOceanRootFolder, customerUid, projectUid.ToString(),
               generatedName, false, null, log, serviceExceptionHandler, dataOceanClient, authn);
            }
          }
        }
      }

      return generatedName;
    }

    #endregion
  }

}
