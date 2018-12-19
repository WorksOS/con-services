using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy for Raptor services.
  /// </summary>
  public class TPaasProxy : BaseProxy, ITPaasProxy
  {
    public TPaasProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    /// <summary>
    /// Gets a new bearer token from TPaaS Oauth
    /// </summary>
    /// <param name="grantType">Project UID</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns></returns>
    public async Task<TPaasOauthResult> Get3DPmSchedulerBearerToken(string grantType, Dictionary<string, string> customHeaders)
    {
      log.LogDebug($"RaptorProxy.Get3dPmSchedulerBearerToken: grantType: {grantType} customHeaders: {JsonConvert.SerializeObject(customHeaders)}");
      var payLoadToSend = $"grant_type={grantType}";
      var tPaasOauthResult = new TPaasOauthResult();
      try
      {
        tPaasOauthResult.tPaasOauthRawResult = await SendRequest<TPaasOauthRawResult>("TPAAS_OAUTH_URL", payLoadToSend, customHeaders, string.Empty, HttpMethod.Post, string.Empty);
      }
      catch (Exception e)
      {
        tPaasOauthResult.Code = 1902; // todo
        tPaasOauthResult.Message = e.Message;
      }
    
      var resultString = tPaasOauthResult == null ? "null" : JsonConvert.SerializeObject(tPaasOauthResult);
      var message = $"TPaasProxy.Get3DPmSchedulerBearerToken: response: {resultString}";
      log.LogDebug(message);

      return tPaasOauthResult;
    }

  }
}