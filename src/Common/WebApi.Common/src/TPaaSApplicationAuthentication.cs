using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Used for authenticating an application in TPaaS e.g. for requests to Trimble Connect, Data Ocean etc.
  /// </summary>
  public class TPaaSApplicationAuthentication : ITPaaSApplicationAuthentication
  {

    private string _applicationBearerToken;
    private const int _refreshPeriodMinutes = 180;
    private DateTime _lastTPaasTokenObtainedUtc = DateTime.UtcNow;
    private IConfigurationStore configuration;
    private ITPaasProxy tpaas;
    private ILogger<TPaaSApplicationAuthentication> Log;
    private object _lock = new object();

    public TPaaSApplicationAuthentication(IConfigurationStore config, ITPaasProxy tpaasProxy, ILogger<TPaaSApplicationAuthentication> log)
    {
      configuration = config;
      tpaas = tpaasProxy;
      Log = log;
    }


    /// <summary>
    /// Gets a temporary bearer token for an application. Refreshes the token as required.
    /// </summary>
    public string GetApplicationBearerToken()
    {
      lock (_lock)
      {
        if (string.IsNullOrEmpty(_applicationBearerToken) ||
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

            tPaasOauthResult = tpaas.GetApplicationBearerToken(grantType, customHeaders).Result;

            Log.LogInformation(
              $"GetApplicationBearerToken() Got new bearer token: TPAAS_OAUTH_URL: {tPaasUrl} grantType: {grantType} customHeaders: {JsonConvert.SerializeObject(customHeaders)}");
          }
          catch (Exception e)
          {
            var message =
              string.Format($"GetApplicationBearerToken call to endpoint failed with exception {e.Message}");

            throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                message));
          }

          if (tPaasOauthResult.Code != 0)
          {
            var message =
              string.Format($"GetApplicationBearerToken call failed with exception {tPaasOauthResult.Message}");
            throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                message));
          }

          _applicationBearerToken = tPaasOauthResult.tPaasOauthRawResult.access_token;
          _lastTPaasTokenObtainedUtc = DateTime.UtcNow;
        }

        Log.LogInformation(
          $"GetApplicationBearerToken()  Using bearer token: {_applicationBearerToken}");
        return _applicationBearerToken;
      }
    }
  }
}

