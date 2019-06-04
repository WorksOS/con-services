using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Mutable.Gateway.Abstractions;

namespace VSS.TRex.Mutable.Gateway.Proxy
{
  public class TRexTagFileProxy : BaseProxy, ITRexTagFileProxy
  {
    private const string TREX_TAGFILE_API_URL_KEY = "TREX_TAGFILE_API_URL";
    private const string CONNECTED_SITE_GATEWAY_URL_KEY = "CONNECTED_SITE_GATEWAY_URL";
    public TRexTagFileProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    public async Task<ContractExecutionResult> SendTagFileDirect(CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendTagFileDirect)}: Filename: {compactionTagFileRequest.FileName}");
      return await SendTagFilePost(TREX_TAGFILE_API_URL_KEY, JsonConvert.SerializeObject(compactionTagFileRequest), customHeaders, "/direct");
    }

    public async Task<ContractExecutionResult> SendTagFileNonDirect(CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendTagFileNonDirect)}: Filename: {compactionTagFileRequest.FileName}");
      return await SendTagFilePost(TREX_TAGFILE_API_URL_KEY, JsonConvert.SerializeObject(compactionTagFileRequest), customHeaders, null);
    }

    public async Task<ContractExecutionResult> SendTagFileNonDirectToConnectedSite(CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SendTagFileNonDirectToConnectedSite)}: Filename: {compactionTagFileRequest.FileName}");
      return await SendTagFilePost(CONNECTED_SITE_GATEWAY_URL_KEY, JsonConvert.SerializeObject(compactionTagFileRequest), customHeaders, null);
    }

    private async Task<ContractExecutionResult> SendTagFilePost(string urlKey, string payload, IDictionary<string, string> customHeaders, string route)
    {
      var response = await SendRequest<ContractExecutionResult>(urlKey, payload, customHeaders, route, HttpMethod.Post, string.Empty);
      log.LogDebug($"{nameof(SendTagFilePost)}: response: {(response == null ? null : JsonConvert.SerializeObject(response))}");

      return response;
    }
  }
}
