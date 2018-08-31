using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using model = VSS.Productivity3D.Models.Models;

namespace VSS.MasterData.Proxies
{
  public class TRexTagFileProxy : BaseProxy, ITRexTagFileProxy
  {
    public TRexTagFileProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    public async Task<ContractExecutionResult> SendTagFileDirect(model.CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexTagFileProxy.SendTagFile: compactionTagFileRequest.Filename: {compactionTagFileRequest.FileName}");
      return await SendTagFilePost(JsonConvert.SerializeObject(compactionTagFileRequest), customHeaders, "/direct");
    }

    public async Task<ContractExecutionResult> SendTagFileNonDirect(model.CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexTagFileProxy.SendTagFile: compactionTagFileRequest.Filename: {compactionTagFileRequest.FileName}");
      return await SendTagFilePost(JsonConvert.SerializeObject(compactionTagFileRequest), customHeaders, null);
    }

    private async Task<ContractExecutionResult> SendTagFilePost(string payload, IDictionary<string, string> customHeaders, string route)
    {
      var response = await SendRequest<ContractExecutionResult>("TREX_TAGFILE_API_URL", payload, customHeaders, route, "POST", String.Empty);
      log.LogDebug("TRexTagFileProxy.SendTagFilePost: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }
  }
}
