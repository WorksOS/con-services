using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class TagFileAuthProjectProxy : BaseProxy, ITagFileAuthProjectProxy
  {
    public TagFileAuthProjectProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    public async Task<GetProjectAndAssetUidsResult> GetProjectAndAssetUids(GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest,
      IDictionary<string, string> customHeaders = null)
    {
      var payload = JsonConvert.SerializeObject(getProjectAndAssetUidsRequest);
      log.LogDebug($"TfaTagFileProxy.GetProjectAndAssetUids: getProjectAndAssetUidsRequest: {payload}");
      var response = await SendRequest<GetProjectAndAssetUidsResult>("TFA_PROJECTV2_API_URL", payload, customHeaders, "/getUids", "POST", String.Empty);

      log.LogDebug("TfaTagFileProxy.GetProjectAndAssetUids: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      return response;
    }

    public async Task<GetProjectAndAssetUidsResult> GetProjectUid(GetProjectUidRequest getProjectUidRequest,
      IDictionary<string, string> customHeaders = null)
    {
      var payload = JsonConvert.SerializeObject(getProjectUidRequest);
      log.LogDebug($"TfaTagFileProxy.GetProjectUid: getProjectUidRequest: {payload}");
      var response = await SendRequest<GetProjectAndAssetUidsResult>("TFA_PROJECTV2_API_URL", payload, customHeaders, "/getUid", "POST", String.Empty);

      log.LogDebug("TfaTagFileProxy.GetProjectUid: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      return response;
    }
  }

}
