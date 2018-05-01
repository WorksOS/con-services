using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Common.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Controller
{
  public class ImportedFileSynchronizerBase
  {
    protected IConfigurationStore ConfigStore;
    protected ILogger Log;
    protected ILoggerFactory Logger;
    protected string FileSpaceId;
    protected IRaptorProxy RaptorProxy;
    protected ITPaasProxy TPaasProxy;
    protected IImportedFileProxy ImpFileProxy;
    protected IFileRepository FileRepo;
    protected bool ProcessSurveyedSurfaceType;

    public static DateTime _lastTPaasTokenObtainedUtc;
    public static string _3DPmSchedulerBearerToken;

    private readonly int _refreshPeriodMinutes = 480;
    private readonly string _3DPmSchedulerConsumerKeys = null;
    private string TemporaryDownloadFolder = null;
    private long MaxFileSize = 0;

    /// <summary>
    /// </summary>
    public ImportedFileSynchronizerBase(IConfigurationStore configStore, ILoggerFactory logger,
      IRaptorProxy raptorProxy, ITPaasProxy tPaasProxy, IImportedFileProxy impFileProxy, IFileRepository fileRepo, bool processSurveyedSurfaceType)
    {
      ConfigStore = configStore;
      Logger = logger;
      Log = logger.CreateLogger<ImportedFileSynchronizerBase>();
      RaptorProxy = raptorProxy;
      TPaasProxy = tPaasProxy;
      ImpFileProxy = impFileProxy;
      FileRepo = fileRepo;
      ProcessSurveyedSurfaceType = processSurveyedSurfaceType;

      FileSpaceId = ConfigStore.GetValueString("TCCFILESPACEID");
      if (string.IsNullOrEmpty(FileSpaceId))
      {
        throw new InvalidOperationException(
          "ImportedFileSynchroniser: unable to establish filespaceId");
      }

      _refreshPeriodMinutes = ConfigStore.GetValueInt("3DPMSCHEDULER_REFRESH_PERIOD_MINUTES");
      if (_refreshPeriodMinutes < 1)
      {
        _refreshPeriodMinutes = 480; // 8 hours
      }

      _3DPmSchedulerConsumerKeys = ConfigStore.GetValueString("3DPMSCHEDULER_CONSUMER_KEYS");
      if (string.IsNullOrEmpty(_3DPmSchedulerConsumerKeys))
      {
        throw new InvalidOperationException(
          "ImportedFileSynchroniser missing from environment variables:3DPMSCHEDULER_CONSUMER_KEYS");
      }

      TemporaryDownloadFolder = ConfigStore.GetValueString("DOWNLOAD_FOLDER");
      if (string.IsNullOrEmpty(TemporaryDownloadFolder))
      {
        var errorString = "Your application is missing an environment variable DOWNLOAD_FOLDER";
        Log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
      if (!TemporaryDownloadFolder.EndsWith("/"))
        TemporaryDownloadFolder += "/";

      Log.LogInformation($"ImportedFileSynchronizerBase: {(ProcessSurveyedSurfaceType ? "processSurveyedSurfaceType" : "processNonSurveyedSurfaceTypes")} FileSpaceId: {FileSpaceId} _refreshPeriodMinutes: {_refreshPeriodMinutes}  _lastTPaasTokenObtainedUtc: {_lastTPaasTokenObtainedUtc} _3DPmSchedulerBearerToken: {_3DPmSchedulerBearerToken} _3DPmSchedulerConsumerKeys: {_3DPmSchedulerConsumerKeys}");
      //Console.WriteLine($"ImportedFileSynchronizerBase: (console temp)  FileSpaceId: {FileSpaceId} _refreshPeriodMinutes: {_refreshPeriodMinutes}  _lastTPaasTokenObtainedUtc: {_lastTPaasTokenObtainedUtc} _3DPmSchedulerBearerToken: {_3DPmSchedulerBearerToken} _3DPmSchedulerConsumerKeys: {_3DPmSchedulerConsumerKeys}");

      MaxFileSize = ConfigStore.GetValueInt("MAX_FILE_SIZE");
      if (MaxFileSize <= 0)
      {
        Log.LogWarning("Missing MAX_FILE_SIZE environment variable so no restriction on downloaded files");
      }
    }


    /// <summary>
    /// Notify raptor of new file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    protected async Task<bool> NotifyRaptorImportedFileChange(string customerUid, Guid projectUid, Guid importedFileUid)
    {
      var startUtc = DateTime.UtcNow;
      var isNotified = false;

      BaseDataResult notificationResult = null;
      var customHeaders = GetCustomHeaders(customerUid);
      try
      {
        notificationResult = await RaptorProxy
          .NotifyImportedFileChange(projectUid, importedFileUid, customHeaders.Result);
      }
      catch (Exception e)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"NotifyImportedFileChange in RaptorServices failed with exception {e.Message}") },
          { "customHeaders", JsonConvert.SerializeObject(customHeaders)},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
      }

      Log.LogInformation(
        $"ImportedFileSynchroniser: NotifyRaptorImportedFileChange() projectUid:{projectUid} importedFileUid: {importedFileUid}. customHeaders: {JsonConvert.SerializeObject(customHeaders)} RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult == null || notificationResult.Code != 0)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"NotifyImportedFileChange in RaptorServices failed. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}") },
          { "customHeaders",JsonConvert.SerializeObject(customHeaders)},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
      }
      else
      {
        isNotified = true;
      }

      return isNotified;
    }

    /// <summary>
    /// Notify raptor of new file
    ///     if it already knows about it, it will just update and re-notify raptor and return success.
    /// </summary>
    /// <returns></returns>
    protected async Task<bool> NotifyRaptorFileDeletedInCGenAsync(string customerUid, Guid projectUid,
      Guid importedFileUid, string fileDescriptor, long importedFileId, long legacyImportedFileId)
    {
      var startUtc = DateTime.UtcNow;
      var isNotified = false;

      BaseDataResult notificationResult = null;
      var customHeaders = GetCustomHeaders(customerUid);
      try
      {
        notificationResult = await RaptorProxy
          .DeleteFile(projectUid, ImportedFileType.SurveyedSurface, importedFileUid, fileDescriptor, importedFileId, legacyImportedFileId, customHeaders.Result);
      }
      catch (Exception e)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"DeleteFile in 3dPmService failed with exception {e.Message}") },
          { "customHeaders", JsonConvert.SerializeObject(customHeaders)},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid},
          { "fileDescriptor", fileDescriptor},
          { "legacyImportedFileId", legacyImportedFileId}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
      }
      Log.LogDebug(
        $"NotifyRaptorFileDeletedInCGen: projectUid:{projectUid} importedFileUid: {importedFileUid} FileDescriptor:{fileDescriptor} legacyImportedFileId {legacyImportedFileId}. 3dPmSerivce returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult == null || notificationResult.Code != 0)
      {
        // proceed with sync, but send alert to NewRelic
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"DeleteFile in 3dPmSerivce failed. Reason: {notificationResult?.Code ?? -1} {notificationResult?.Message ?? "null"}") },
          { "customHeaders", JsonConvert.SerializeObject(customHeaders)},
          { "projectUid", projectUid},
          { "importedFileUid", importedFileUid},
          { "fileDescriptor", fileDescriptor},
          { "legacyImportedFileId", legacyImportedFileId}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
      }
      else
      {
        isNotified = true;
      }

      return isNotified;
    }

    private async Task<IDictionary<string, string>> GetCustomHeaders(string customerUid)
    {
      var customHeaders = new Dictionary<string, string>();

      string bearerToken = await Get3DPmSchedulerBearerToken();
      customHeaders.Add("X-VisionLink-CustomerUid", customerUid);
      customHeaders.Add("Authorization", string.Format($"Bearer {bearerToken}"));
      customHeaders.Add("X-VisionLink-ClearCache", "true");

      return customHeaders;
    }

    
    public async Task<string> Get3DPmSchedulerBearerToken()
    {
      var startUtc = DateTime.UtcNow;

      if (string.IsNullOrEmpty(_3DPmSchedulerBearerToken) ||
          (DateTime.UtcNow - _lastTPaasTokenObtainedUtc).TotalMinutes > _refreshPeriodMinutes)
      {
        var customHeaders = new Dictionary<string, string>
        {
          {"Accept", "application/json"},
          {"Content-Type", "application/x-www-form-urlencoded"},
          {"Authorization", string.Format($"Bearer {_3DPmSchedulerConsumerKeys}")}
        };
        string grantType = "client_credentials";
        TPaasOauthResult tPaasOauthResult;

        try
        {
          
          var tPaasUrl = ConfigStore.GetValueString("TPAAS_OAUTH_URL") == null ? "null" : ConfigStore.GetValueString("TPAAS_OAUTH_URL");
 
          tPaasOauthResult = await TPaasProxy
            .Get3DPmSchedulerBearerToken(grantType, customHeaders);

          Log.LogInformation($"ImportedFileSynchroniser: Get3DPmSchedulerBearerToken() Got new bearer token: TPAAS_OAUTH_URL: {tPaasUrl} grantType: {grantType} customHeaders: {JsonConvert.SerializeObject(customHeaders)}");
          //Console.WriteLine($"ImportedFileSynchroniser: Get3DPmSchedulerBearerToken() (console temp) Got new bearer token: TPAAS_OAUTH_URL: {tPaasUrl} grantType: {grantType} customHeaders: {JsonConvert.SerializeObject(customHeaders)}");
        }
        catch (Exception e)
        {
          var message = string.Format($"Get3dPmSchedulerBearerToken call to endpoint failed with exception {e.Message}");
          var newRelicAttributes = new Dictionary<string, object> {
            { "message", message},
            { "customHeaders", JsonConvert.SerializeObject(customHeaders)},
            { "grantType", grantType}
          };
          NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              message));
        }

        if (tPaasOauthResult.Code != 0)
        {
          var message = string.Format($"Get3dPmSchedulerBearerToken call failed with exception {tPaasOauthResult.Message}");
          var newRelicAttributes = new Dictionary<string, object> {
            { "message", message},
            { "customHeaders", JsonConvert.SerializeObject(customHeaders)},
            { "tPaasOauthResult", JsonConvert.SerializeObject(tPaasOauthResult)},
            { "grantType", grantType}
          };
          NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              message));
        }
        _3DPmSchedulerBearerToken = tPaasOauthResult.tPaasOauthRawResult.access_token;
        _lastTPaasTokenObtainedUtc = DateTime.UtcNow;
      }

      Log.LogInformation($"ImportedFileSynchroniser: Get3dPmSchedulerBearerToken()  Using bearer token: {_3DPmSchedulerBearerToken}");
      //Console.WriteLine($"ImportedFileSynchroniser: Get3dPmSchedulerBearerToken() (console temp)  Using bearer token: {_3DPmSchedulerBearerToken}");
      return _3DPmSchedulerBearerToken;
    }

    /// <summary>
    /// Downloads a file from TCC, saves it to a temporary folder then calls the project web api
    /// to import it into Project.
    /// </summary>
    /// <param name="projectEvent"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    protected async Task<BaseDataResult> DownloadFileAndCallProjectWebApi(ImportedFileProject projectEvent, WebApiAction action)
    {
      //TODO: If performance is a problem then may need to add 'Copy' command to TCCFileAccess 
      //and use it here to directly copy file from old to new structure in TCC.

      Log.LogInformation($"ImportedFileSynchroniser: DownloadFileAndCallProjectWebApi");
      BaseDataResult result = null;

      var fileDescriptor = JsonConvert.DeserializeObject<SchedulerFileDescriptor>(projectEvent.FileDescriptor);
      if (await DownloadFileAndSaveToTemp(fileDescriptor))
      {
        result = await CallProjectWebApi(projectEvent, action, fileDescriptor);
 
        try
        {
          //Clean up - delete downloaded file
          Log.LogInformation($"ImportedFileSynchroniser: Deleting temporary file {FullTemporaryFileName(fileDescriptor)}");
          File.Delete(FullTemporaryFileName(fileDescriptor));
        }
        catch (Exception)
        {
          //We don't care really
        }
   
      }
      return result;
    }

    /// <summary>
    /// Call the project web api to import the file.
    /// </summary>
    /// <param name="projectEvent"></param>
    /// <param name="action"></param>
    /// <param name="fileDescriptor"></param>
    /// <returns></returns>
    protected async Task<BaseDataResult> CallProjectWebApi(ImportedFileProject projectEvent, WebApiAction action, SchedulerFileDescriptor fileDescriptor)
    {
      string errorMessage = null;
      BaseDataResult result = null;

      var startUtc = DateTime.UtcNow;
      string fullName = FullTemporaryFileName(fileDescriptor);
      if (action == WebApiAction.Deleting)
      {
        //Remove temporary download folder part of the path
        fullName = fullName.Substring(TemporaryDownloadFolder.Length);
      }
      var projectUid = Guid.Parse(projectEvent.ProjectUid);
      var customHeaders = await GetCustomHeaders(projectEvent.CustomerUid);
      try
      {
        Log.LogInformation($"ImportedFileSynchroniser: Calling project web api {fullName}");
        switch (action)
        {
          case WebApiAction.Creating:
           result = await ImpFileProxy.CreateImportedFile(fullName, fileDescriptor.fileName,
              projectUid, projectEvent.ImportedFileType, projectEvent.FileCreatedUtc, 
              projectEvent.FileUpdatedUtc, projectEvent.DxfUnitsType,
              projectEvent.SurveyedUtc, customHeaders);
            break;
          case WebApiAction.Updating:
            result = await ImpFileProxy.UpdateImportedFile(fullName, fileDescriptor.fileName,
              projectUid, projectEvent.ImportedFileType, projectEvent.FileCreatedUtc, 
              projectEvent.FileUpdatedUtc, projectEvent.DxfUnitsType, 
              projectEvent.SurveyedUtc, customHeaders);
            break;
          case WebApiAction.Deleting:
            result = await ImpFileProxy.DeleteImportedFile(projectUid, 
              Guid.Parse(projectEvent.ImportedFileUid), customHeaders);
            break;
        }
        if (result.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        {
          errorMessage = $"CallProjectWebApi call failed with result code {result.Code} and message {result.Message}";
        }
      }
      catch (Exception e)
      {
        errorMessage = $"CallProjectWebApi call failed with exception {e.Message}"; 
      }
      if (!string.IsNullOrEmpty(errorMessage))
      {
        var newRelicAttributes = new Dictionary<string, object> {
          { "message", errorMessage},
          { "customHeaders", JsonConvert.SerializeObject(customHeaders)},
          { "action", action},
          { "fullFileName", fullName}
        };
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
      }
      return result;
    }

    /// <summary>
    /// Tries to download a file from TCC and saves it to a temporary location
    /// </summary>
    private async Task<bool> DownloadFileAndSaveToTemp(SchedulerFileDescriptor fileDescriptor)
    {
      var startUtc = DateTime.UtcNow;
      var result = false;
      var fullFileName = $"{fileDescriptor.path}/{fileDescriptor.fileName}";
      Log.LogInformation($"ImportedFileSynchroniser: DownloadFileAndSaveToTemp {fullFileName}");

      if (await FileRepo.FileExists(fileDescriptor.filespaceId, fullFileName))
      {
        bool ok = true;
        if (MaxFileSize > 0)
        {
          //Ignore very big files as will fail to download with timeout. We will fix them manually.
          var fileList = await FileRepo.GetFileList(fileDescriptor.filespaceId, fileDescriptor.path, Path.GetExtension(fileDescriptor.fileName));
          var fileSize = fileList?.entries.SingleOrDefault(f => f.entryName == fileDescriptor.fileName)?.size;
          ok = fileSize <= MaxFileSize;
          if (!ok)
          {
            var message = $"ImportedFileSynchroniser: File {fullFileName} ignored as too big";
            Log.LogWarning(message);
            var newRelicAttributes = new Dictionary<string, object>
            {
              {"message", message},
              {"fullFileName", fullFileName}
            };
            NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
          }
        }
        if (ok)
        {
          using (Stream inStream = await FileRepo.GetFile(fileDescriptor.filespaceId, fullFileName))
          {
            if (inStream.Length > 0)
            {
              inStream.Position = 0;

              try
              {
                Log.LogInformation($"ImportedFileSynchroniser: Saving to temporary file {fullFileName}");

                var path = FullTemporaryPath(fileDescriptor.path);
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (!dirInfo.Exists)
                {
                  try
                  {
                    dirInfo.Create();
                  }
                  catch (Exception e)
                  {
                    Log.LogWarning($"Failed to create temporary download folder {path}: {e.Message}");
                    throw;
                  }
                }

                using (Stream outStream = File.Create($"{FullTemporaryFileName(fileDescriptor)}"))
                {
                  inStream.CopyTo(outStream);
                  result = true;
                }
              }
              catch (Exception e)
              {
                var message = string.Format($"DownloadFileAndSaveToTemp call failed with exception {e.Message}");
                var newRelicAttributes = new Dictionary<string, object>
                {
                  {"message", message},
                  {"fullTemporaryFileName", FullTemporaryFileName(fileDescriptor)}
                };
                NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, Log, newRelicAttributes);
              }
            }
          }
        }
      }
      else
      {
        Log.LogWarning($"ImportedFileSynchroniser: DownloadFileAndSaveToTemp {fullFileName} not found!");
      }
      Log.LogInformation($"ImportedFileSynchroniser: DownloadFileAndSaveToTemp returning {result} for {fullFileName} ");

      return result;
    }

    private string FullTemporaryPath(string filePath)
    {
      return $"{TemporaryDownloadFolder}{filePath}";
    }
    private string FullTemporaryFileName(SchedulerFileDescriptor fileDescriptor)
    {
      return $"{FullTemporaryPath(fileDescriptor.path)}/{fileDescriptor.fileName}";
    }

    protected enum WebApiAction
    {
      Creating,
      Updating,
      Deleting
    }

  }
}
