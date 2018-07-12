using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace Common.netstandard.ApiClients
{
  public class _3dpmAuthN : I_3dpmAuthN
  {

    private string _3DPmSchedulerBearerToken;
    private const int _refreshPeriodMinutes = 180;
    private DateTime _lastTPaasTokenObtainedUtc = DateTime.UtcNow;
    private IConfigurationStore configuration;
    private ITPaasProxy tpaas;
    private ILogger<_3dpmAuthN> Log;
    private object _lock = new object();

    public _3dpmAuthN(IConfigurationStore config, ITPaasProxy tpaasProxy, ILogger<_3dpmAuthN> log )
    {
      configuration = config;
      tpaas = tpaasProxy;
      Log = log;
    }


    public async Task<string> Get3DPmSchedulerBearerToken()
    {
      lock (_lock)
      {
        var startUtc = DateTime.UtcNow;

        if (string.IsNullOrEmpty(_3DPmSchedulerBearerToken) ||
            (DateTime.UtcNow - _lastTPaasTokenObtainedUtc).TotalMinutes > _refreshPeriodMinutes)
        {
          var customHeaders = new Dictionary<string, string>
          {
            {"Accept", "application/json"},
            {"Content-Type", "application/x-www-form-urlencoded"},
            {"Authorization", string.Format($"Bearer {configuration.GetValueString("TPAAS_APP_TOKENKEYS")}")}
          };
          string grantType = "client_credentials";
          TPaasOauthResult tPaasOauthResult;

          try
          {

            var tPaasUrl = configuration.GetValueString("TPAAS_OAUTH_URL") == null
              ? "null"
              : configuration.GetValueString("TPAAS_OAUTH_URL");

            tPaasOauthResult = tpaas.Get3DPmSchedulerBearerToken(grantType, customHeaders).Result;

            Log.LogInformation(
              $"Get3DPmSchedulerBearerToken() Got new bearer token: TPAAS_OAUTH_URL: {tPaasUrl} grantType: {grantType} customHeaders: {JsonConvert.SerializeObject(customHeaders)}");
          }
          catch (Exception e)
          {
            var message =
              string.Format($"Get3dPmSchedulerBearerToken call to endpoint failed with exception {e.Message}");

            throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                message));
          }

          if (tPaasOauthResult.Code != 0)
          {
            var message =
              string.Format($"Get3dPmSchedulerBearerToken call failed with exception {tPaasOauthResult.Message}");
            throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                message));
          }

          _3DPmSchedulerBearerToken = tPaasOauthResult.tPaasOauthRawResult.access_token;
          _lastTPaasTokenObtainedUtc = DateTime.UtcNow;
        }

        Log.LogInformation(
          $"Get3dPmSchedulerBearerToken()  Using bearer token: {_3DPmSchedulerBearerToken}");
        return _3DPmSchedulerBearerToken;
      }
    }
  }
}
