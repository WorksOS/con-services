using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public static class DataOceanHelper
  {
    /// <summary>
    /// Writes the importedFile to DataOcean as a create or update,
    ///       if it already exists, old version will be deleted first.
    /// </summary>
    public static async Task WriteFileToDataOcean(
      Stream fileContents, string rootFolder, string customerUid, string projectUid, string dataOceanFileName,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IDataOceanClient dataOceanClient,
      ITPaaSApplicationAuthentication authn, Guid fileUid, IConfigurationStore configStore)
    {
      var dataOceanEnabled = configStore.GetValueBool("ENABLE_DATA_OCEAN", false);

      if (dataOceanEnabled)
      {
        if (!dataOceanFileName.StartsWith(fileUid.ToString()))
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              $"Invalid DataOcean file name {dataOceanFileName}"));
        }

        var customHeaders = authn.CustomHeaders();
        var dataOceanPath = DataOceanFileUtil.DataOceanPath(rootFolder, customerUid, projectUid);

        var ccPutFileResult = false;
        var folderAlreadyExists = false;

        try
        {
          log.LogInformation($"{nameof(WriteFileToDataOcean)}: dataOceanPath: '{dataOceanPath}', dataOceanFileName: '{dataOceanFileName}'");

          folderAlreadyExists = await dataOceanClient.FolderExists(dataOceanPath, customHeaders);

          if (!folderAlreadyExists)
          {
            await dataOceanClient.MakeFolder(dataOceanPath, customHeaders);
          }

          ccPutFileResult = await dataOceanClient.PutFile(dataOceanPath, dataOceanFileName, fileContents, customHeaders);
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "dataOceanClient.PutFile",
            e.Message);
        }

        if (!ccPutFileResult)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 116);
        }

        log.LogInformation($"{nameof(WriteFileToDataOcean)}: dataOceanFileName '{dataOceanFileName}' written to DataOcean, folderAlreadyExists: {folderAlreadyExists}");
      }
      else
      {
        log.LogInformation($"{nameof(WriteFileToDataOcean)}: File not saved. DataOcean disabled");
      }
    }

    /// <summary>
    /// Deletes the importedFile from DataOcean
    /// </summary>
    public static async Task<ImportedFileInternalResult> DeleteFileFromDataOcean(
      string fileName, string rootFolder, string customerUid, Guid projectUid, Guid importedFileUid,
      ILogger log, IDataOceanClient dataOceanClient, ITPaaSApplicationAuthentication authn, IConfigurationStore configStore)
    {
      var dataOceanEnabled = configStore.GetValueBool("ENABLE_DATA_OCEAN", false);
      if (dataOceanEnabled)
      {
        var dataOceanPath = DataOceanFileUtil.DataOceanPath(rootFolder, customerUid, projectUid.ToString());
        var fullFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{fileName}";
        log.LogInformation($"{nameof(DeleteFileFromDataOcean)}: fullFileName {JsonConvert.SerializeObject(fullFileName)}");

        var customHeaders = authn.CustomHeaders();
        bool ccDeleteFileResult;

        try
        {
          ccDeleteFileResult = await dataOceanClient.DeleteFile(fullFileName, customHeaders);
        }
        catch (Exception e)
        {
          log.LogError(e, $"{nameof(DeleteFileFromDataOcean)}: failed for {fileName} (importedFileUid:{importedFileUid}) with exception {e.Message}");
          return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 57, "dataOceanClient.DeleteFile", e.Message);
        }

        if (!ccDeleteFileResult)
        {
          log.LogWarning(
            $"{nameof(DeleteFileFromDataOcean)}: failed to delete {fileName} (importedFileUid:{importedFileUid}).");
          //Not an error if it doesn't delete the file?
          //return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 117);
        }
      }
      else
      {
        log.LogInformation($"{nameof(DeleteFileFromDataOcean)}: File not deleted. DataOcean disabled");
      }

      return null;
    }

    public static IHeaderDictionary CustomHeaders(ITPaaSApplicationAuthentication authn) => new HeaderDictionary
    {
      {"Content-Type", ContentTypeConstants.ApplicationJson},
      {"Authorization", $"Bearer {authn.GetApplicationBearerToken()}"},
      {"Accept", "*/*" }
    };
  }
}
