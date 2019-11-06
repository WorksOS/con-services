using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class TpaasEmailProxy : BaseServiceDiscoveryProxy, ITpaasEmailProxy
  {
    public TpaasEmailProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    {
    }
    // https://docs.google.com/document/d/1tzktOSNUboEyD64WGV8ZkBDULBU9slHGNo_ZLPvdNY0

    public override bool IsInsideAuthBoundary => false;
    public override ApiService InternalServiceType => ApiService.None;
    public override string ExternalServiceName => "tpaasemail";
    public override ApiVersion Version => ApiVersion.V1;
    public override ApiType Type => ApiType.Public;
    public override string CacheLifeKey => String.Empty;

    public Task<Stream> SendEmail(EmailModel emailModel, IDictionary<string, string> customHeaders = null)
    {
      var payloadToSend = JsonConvert.SerializeObject(emailModel);
      log.LogDebug($"SendEmail: {JsonConvert.SerializeObject(emailModel)}");
      return GetMasterDataStreamItemServiceDiscoveryNoCache("", customHeaders, HttpMethod.Post, payload: payloadToSend);


    }
  }
}
