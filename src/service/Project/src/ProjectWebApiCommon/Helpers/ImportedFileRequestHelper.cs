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

    #region tiles
    /// <summary>
    /// Create a DXF file of the alignment center line using Raptor and save it to data ocean.
    /// </summary>
    /// <returns>The generated file name</returns>
    public static async Task<string> CreateGeneratedDxfFile(string customerUid, Guid projectUid, Guid alignmentUid, IProductivity3dV2ProxyCompaction productivity3DV2ProxyCompaction,
      IHeaderDictionary headers, ILogger log, IServiceExceptionHandler serviceExceptionHandler, ITPaaSApplicationAuthentication authn,
      IDataOceanClient dataOceanClient, IConfigurationStore configStore, string fileName, string rootFolder)
    {
      var generatedName = DataOceanFileUtil.GeneratedFileName(fileName, ImportedFileType.Alignment);
      // TODO - As we do not receive .DXF file contents and insted we get the collection of arrays of vertices 
      // describing a poly line representation of the alignment center line then we need to revist the code bellow.
      /*
      //Get generated DXF file from Raptor
      var dxfContents = await productivity3DV2ProxyCompaction.GetLineworkFromAlignment(projectUid, alignmentUid, headers);
      //GracefulWebRequest should throw an exception if the web api call fails but just in case...
      if (dxfContents != null && dxfContents.Length > 0)
      {
        //Unzip it and save to DataOcean 
        using (var archive = new ZipArchive(dxfContents))
        {
          if (archive.Entries.Count == 1)
          {
            using (var stream = archive.Entries[0].Open() as DeflateStream)
            using (var ms = new MemoryStream())
            {
              // Unzip the file, copy to memory 
              stream.CopyTo(ms);
              ms.Seek(0, SeekOrigin.Begin);
              await DataOceanHelper.WriteFileToDataOcean(
                ms, rootFolder, customerUid, projectUid.ToString(), generatedName, log, serviceExceptionHandler, dataOceanClient, authn, alignmentUid, configStore);
            }
          }
        }
      }*/

      return generatedName;
    }

    #endregion
  }
}
