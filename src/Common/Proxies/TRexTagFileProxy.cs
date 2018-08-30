using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Common.Proxies
{
  public class TRexTagFileProxy : BaseProxy, ITRexTagFileProxy
  {
    public TRexTagFileProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    // todo does this need any cache? fielname?

    public async Task<ContractExecutionResult> SendTagFileDirect(CompactionTagFileRequest compactionTagFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"TRexTagFileProxy.SendTagFile: compactionTagFileRequest.Filename: {compactionTagFileRequest.FileName}");
      return await SendTagFilePost(JsonConvert.SerializeObject(compactionTagFileRequest), customHeaders, "/direct");

    }

    private async Task<ContractExecutionResult> SendTagFilePost(string payload, IDictionary<string, string> customHeaders, string route)
    {
      var response = await SendRequest<ContractExecutionResult>("TREXGATEWAY_API_URL", payload, customHeaders, route, "POST", String.Empty);
      log.LogDebug("TRexTagFileProxy.SendTagFilePost: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }
  }
}
