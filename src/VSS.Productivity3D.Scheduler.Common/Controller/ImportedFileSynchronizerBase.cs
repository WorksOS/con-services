using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Scheduler.Common.Utilities;

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

    private DateTime _lastTPaasTokenObtainedUtc = DateTime.MinValue;
    private readonly int _refreshPeriodMinutes = 480;
    private readonly string _3DPmSchedulerConsumerKeys = null;
    private string _3DPmSchedulerBearerToken = null;

    /// <summary>
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="raptorProxy"></param>
    public ImportedFileSynchronizerBase(IConfigurationStore configStore, ILoggerFactory logger,
      IRaptorProxy raptorProxy, ITPaasProxy tPaasProxy)
    {
      ConfigStore = configStore;
      Logger = logger;
      Log = logger.CreateLogger<ImportedFileSynchronizerBase>();
      RaptorProxy = raptorProxy;
      TPaasProxy = tPaasProxy;

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
          .NotifyImportedFileChange(projectUid, importedFileUid, customHeaders.Result)
          .ConfigureAwait(false);
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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, Log, newRelicAttributes);
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
        NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, Log, newRelicAttributes);
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

      string bearerToken = await Get3DPmSchedulerBearerToken().ConfigureAwait(false);
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
            .Get3DPmSchedulerBearerToken(grantType, customHeaders)
            .ConfigureAwait(false);

          Log.LogInformation($"ImportedFileSynchroniser: Get3DPmSchedulerBearerToken() Going to get bearer token: TPAAS_OAUTH_URL: {tPaasUrl} grantType: {grantType} customHeaders: {JsonConvert.SerializeObject(customHeaders)} _lastTPaasTokenObtainedUtc: {_lastTPaasTokenObtainedUtc} _refreshPeriodMinutes: {_refreshPeriodMinutes} DateTime.UtcNow: {DateTime.UtcNow}");
        }
        catch (Exception e)
        {
          var message = string.Format($"Get3dPmSchedulerBearerToken call to endpoint failed with exception {e.Message}");
          var newRelicAttributes = new Dictionary<string, object> {
            { "message", message},
            { "customHeaders", JsonConvert.SerializeObject(customHeaders)},
            { "grantType", grantType}
          };
          NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, Log, newRelicAttributes);
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
          NewRelicUtils.NotifyNewRelic("ImportedFilesSyncTask", "Error", startUtc, (DateTime.UtcNow - startUtc).TotalMilliseconds, Log, newRelicAttributes);
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              message));
        }
        _3DPmSchedulerBearerToken = tPaasOauthResult.tPaasOauthRawResult.access_token;
        _lastTPaasTokenObtainedUtc = DateTime.UtcNow;
      }

      Log.LogInformation($"ImportedFileSynchroniser: Get3dPmSchedulerBearerToken()  Got bearer token: {_3DPmSchedulerBearerToken}");
      return _3DPmSchedulerBearerToken;
    }
  }
}
