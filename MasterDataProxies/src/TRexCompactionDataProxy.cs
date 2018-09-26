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
  public class TRexCompactionDataProxy : BaseProxy, ITRexCompactionDataProxy
  {
    protected TRexCompactionDataProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    {
    }

    public async Task<ContractExecutionResult> SendCMVChangeDetailsRequest(CMVChangeDetailsRequest cmvChangeDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendCMVChangeDetailsRequest)}: Sending the request: {cmvChangeDetailsRequest}");

      return await SendRequestPost(JsonConvert.SerializeObject(cmvChangeDetailsRequest), customHeaders, "/cmv/percentchange");
    }

    public async Task<ContractExecutionResult> SendCMVDetailsRequest(CMVDetailsRequest cmvDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendCMVDetailsRequest)}: Sending the request: {cmvDetailsRequest}");

      return await SendRequestPost(JsonConvert.SerializeObject(cmvDetailsRequest), customHeaders, "/cmv/details");
    }

    private async Task<ContractExecutionResult> SendRequestPost(string payload, IDictionary<string, string> customHeaders, string route)
    {
      var response = await SendRequest<ContractExecutionResult>("TREX_GATEWAY_API_URL", payload, customHeaders, route, "POST", string.Empty);

      log.LogDebug($"{nameof(SendRequestPost)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }
  }
}
