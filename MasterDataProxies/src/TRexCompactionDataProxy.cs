using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy to access the TRex Gateway WebAPIs.
  /// </summary>
  public class TRexCompactionDataProxy : BaseProxy, ITRexCompactionDataProxy
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="configurationStore"></param>
    /// <param name="logger"></param>
    protected TRexCompactionDataProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    {
    }

    /// <summary>
    /// Sends a request to get CMV % Change statistics from the TRex database.
    /// </summary>
    /// <param name="cmvChangeDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<ContractExecutionResult> SendCMVChangeDetailsRequest(CMVChangeDetailsRequest cmvChangeDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendCMVChangeDetailsRequest)}: Sending the request: {cmvChangeDetailsRequest}");

      return await SendRequestPost(JsonConvert.SerializeObject(cmvChangeDetailsRequest), customHeaders, "/cmv/percentchange");
    }

    /// <summary>
    /// Sends a request to get CMV Details statistics from the TRex database.
    /// </summary>
    /// <param name="cmvDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<ContractExecutionResult> SendCMVDetailsRequest(CMVDetailsRequest cmvDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendCMVDetailsRequest)}: Sending the request: {cmvDetailsRequest}");

      return await SendRequestPost(JsonConvert.SerializeObject(cmvDetailsRequest), customHeaders, "/cmv/details");
    }

    /// <summary>
    /// Sends a request to get Pass Count Details statistics from the TRex database.
    /// </summary>
    /// <param name="pcDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<ContractExecutionResult> SendPassCountDetailsRequest(PassCountDetailsRequest pcDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendPassCountDetailsRequest)}: Sending the request: {pcDetailsRequest}");

      return await SendRequestPost(JsonConvert.SerializeObject(pcDetailsRequest), customHeaders, "/passcounts/details");
    }

    /// <summary>
    /// Sends a request to get Cut/Fill Details statistics from the TRex database.
    /// </summary>
    /// <param name="cfDetailsRequest"></param>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<ContractExecutionResult> SendCutFillDetailsRequest(CutFillDetailsRequest cfDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendCutFillDetailsRequest)}: Sending the request: {cfDetailsRequest}");

      return await SendRequestPost(JsonConvert.SerializeObject(cfDetailsRequest), customHeaders, "/cutfill/details");
    }

    /// <summary>
    /// Executes a POST request against the TRex Gateway service.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="customHeaders"></param>
    /// <param name="route"></param>
    /// <returns></returns>
    private async Task<ContractExecutionResult> SendRequestPost(string payload, IDictionary<string, string> customHeaders, string route)
    {
      var response = await SendRequest<ContractExecutionResult>("TREX_GATEWAY_API_URL", payload, customHeaders, route, "POST", string.Empty);

      log.LogDebug($"{nameof(SendRequestPost)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }
  }
}
