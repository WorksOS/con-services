using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Used for authenticating an application in TPaaS e.g. for requests to Trimble Connect, Data Ocean etc.
  /// </summary>
  public class TPaaSApplicationAuthentication : ITPaaSApplicationAuthentication
  {

    private string _applicationBearerToken;
    private DateTime _tPaasTokenExpiryUtc = DateTime.MinValue;
    private IConfigurationStore configuration;
    private ITPaasProxy tpaas;
    private ILogger<TPaaSApplicationAuthentication> Log;
    private object _lock = new object();
    private const int DefaultLogMaxChar = 1000;
    private readonly int _logMaxChar;

    public TPaaSApplicationAuthentication(IConfigurationStore config, ITPaasProxy tpaasProxy, ILogger<TPaaSApplicationAuthentication> log)
    {
      configuration = config;
      tpaas = tpaasProxy;
      Log = log;
      _logMaxChar = configuration.GetValueInt("LOG_MAX_CHAR", DefaultLogMaxChar);
    }


    /// <summary>
    /// Gets a temporary bearer token for an application. Refreshes the token as required.
    /// </summary>
    public string GetApplicationBearerToken()
    {
      const int TOKEN_EXPIRY_GRACE_SECONDS = 60;
      const string grantType = "client_credentials";

      lock (_lock)
      {
        if (string.IsNullOrEmpty(_applicationBearerToken) ||
            _tPaasTokenExpiryUtc < DateTime.UtcNow)
        {
          var customHeaders = new Dictionary<string, string>
          {
            {"Accept", ContentTypeConstants.ApplicationJson},
            {"Content-Type", ContentTypeConstants.ApplicationFormUrlEncoded},
            {"Authorization", string.Format($"Basic {configuration.GetValueString("TPAAS_APP_TOKENKEYS")}")}
          };
          TPaasOauthResult tPaasOauthResult; 

          try
          {
            //Revoke expired or expiring token
            if (!string.IsNullOrEmpty(_applicationBearerToken))
            {
              var revokeResult = tpaas.RevokeApplicationBearerToken(_applicationBearerToken, customHeaders).Result;
              if (revokeResult.Code != 0)
              {
                Log.LogInformation($"GetApplicationBearerToken failed to revoke token: {revokeResult.Message}");
                throw new ServiceException(HttpStatusCode.InternalServerError,
                  new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                    $"Failed to revoke application bearer token: {revokeResult.Message}"));
              }

              _applicationBearerToken = null;
              _tPaasTokenExpiryUtc = DateTime.MinValue;
            }
            //Authenticate to get a token
            tPaasOauthResult = tpaas.GetApplicationBearerToken(grantType, customHeaders).Result;

            var tPaasUrl = configuration.GetValueString("TPAAS_OAUTH_URL") ?? "null";
            Log.LogInformation(
              $"GetApplicationBearerToken() Got new bearer token: TPAAS_OAUTH_URL: {tPaasUrl} grantType: {grantType} customHeaders: {customHeaders.LogHeaders(_logMaxChar)}");
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
          _tPaasTokenExpiryUtc = DateTime.UtcNow.AddSeconds(tPaasOauthResult.tPaasOauthRawResult.expires_in - TOKEN_EXPIRY_GRACE_SECONDS);
        }

        Log.LogInformation(
          $"GetApplicationBearerToken()  Using bearer token: {_applicationBearerToken}");
        return _applicationBearerToken;
      }
    }

    public IDictionary<string, string> CustomHeaders()
    {
      return new Dictionary<string, string>
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {GetApplicationBearerToken()}"}
      };
    }
  }
}

