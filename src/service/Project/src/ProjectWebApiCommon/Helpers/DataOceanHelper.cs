using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public class DataOceanHelper
  {
    /// <summary>
    /// Construct the path in DataOcean
    /// </summary>
    public static string DataOceanPath(string rootFolder, string customerUid, string projectUid)
    {
      return $"{Path.DirectorySeparatorChar}{rootFolder}{Path.DirectorySeparatorChar}{customerUid}{Path.DirectorySeparatorChar}{projectUid}";
    }
    /// <summary>
    /// Writes the importedFile to DataOcean
    ///   this may be a create or update, so ok if it already exists already
    /// </summary>
    public static async Task WriteFileToDataOcean(
      Stream fileContents, string rootFolder, string customerUid, string projectUid,
      string pathAndFileName, bool isSurveyedSurface, DateTime? surveyedUtc,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, IDataOceanClient dataOceanClient,
      ITPaaSApplicationAuthentication authn)
    {
      var customHeaders = CustomHeaders(authn);
      var dataOceanPath = DataOceanPath(rootFolder, customerUid, projectUid);
      string dataOceanFileName = Path.GetFileName(pathAndFileName);

      //TODO: DataOcean has versions of files. We should leverage that rather than appending surveyed UTC to file name.
      if (isSurveyedSurface && surveyedUtc != null) // validation should prevent this
        dataOceanFileName = ImportedFileUtils.IncludeSurveyedUtcInName(dataOceanFileName, surveyedUtc.Value);

      bool ccPutFileResult = false;
      bool folderAlreadyExists = false;
      try
      {
        log.LogInformation(
          $"WriteFileToDataOcean: dataOceanPath {dataOceanPath} dataOceanFileName {dataOceanFileName}");
        folderAlreadyExists = await dataOceanClient.FolderExists(dataOceanPath, customHeaders);
        if (folderAlreadyExists == false)
          await dataOceanClient.MakeFolder(dataOceanPath, customHeaders);

        // this does an upsert
        ccPutFileResult = await dataOceanClient.PutFile(dataOceanPath, dataOceanFileName, fileContents, customHeaders);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, "dataOceanClient.PutFile",
          e.Message);
      }

      if (ccPutFileResult == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 116);
      }

      log.LogInformation(
        $"WriteFileToDataOcean: dataOceanFileName {dataOceanFileName} written to DataOcean. folderAlreadyExists {folderAlreadyExists}");
    }

    /// <summary>
    /// Deletes the importedFile from DataOcean
    /// </summary>
    /// <returns></returns>
    public static async Task<ImportedFileInternalResult> DeleteFileFromDataOcean(
      string fileName, string rootFolder, string customerUid, Guid projectUid, Guid importedFileUid, 
      ILogger log, IServiceExceptionHandler serviceExceptionHandler, 
      IDataOceanClient dataOceanClient, ITPaaSApplicationAuthentication authn)
    {
      var dataOceanPath = DataOceanPath(rootFolder, customerUid, projectUid.ToString());
      var fullFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{fileName}";
      log.LogInformation($"DeleteFileFromDataOcean: fullFileName {JsonConvert.SerializeObject(fullFileName)}");

      var customHeaders = CustomHeaders(authn);
      bool ccDeleteFileResult = false;
      try
      {
        ccDeleteFileResult = await dataOceanClient.DeleteFile(fullFileName, customHeaders);
      }
      catch (Exception e)
      {
        log.LogError(e, $"DeleteFileFromDataOcean failed for {fileName} (importedFileUid:{importedFileUid}) with exception {e.Message}");
        return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 57, "dataOceanClient.DeleteFile", e.Message);
      }

      if (ccDeleteFileResult == false)
      {
        log.LogWarning(
          $"DeleteFileFromDataOcean failed to delete {fileName} (importedFileUid:{importedFileUid}).");
        //Not an error if it doesn't delete the file?
        //return ImportedFileInternalResult.CreateImportedFileInternalResult(HttpStatusCode.InternalServerError, 117);
      }
    
      return null;
    }

    private static IDictionary<string, string> CustomHeaders(ITPaaSApplicationAuthentication authn)
    {
      return new Dictionary<string, string>
      {
        {"Content-Type", "application/json"},
        {"Authorization", $"Bearer {authn.GetApplicationBearerToken()}"},
        {"Accept", "*/*" }
      };
    }
    
  }
}
