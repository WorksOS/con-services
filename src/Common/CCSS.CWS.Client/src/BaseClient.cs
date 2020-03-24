using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  //TODO: each client (derived class) will implement its own interface
  public abstract class BaseClient : BaseServiceDiscoveryProxy
  {
    public override bool IsInsideAuthBoundary => false;
    public override ApiService InternalServiceType => ApiService.None;
    public override string ExternalServiceName => "cws";
    public override ApiVersion Version => ApiVersion.V1;
    public override ApiType Type => ApiType.Public;
    public override string CacheLifeKey => "CWS_CACHE_LIFE";

     protected BaseClient(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    {
    }

    // NOTE: must have a uid or userId for cache key
    protected Task<TRes> GetData<TRes>(string route, string uid, string userId,
      IList<KeyValuePair<string, string>> parameters = null,
      IDictionary<string, string> customHeaders = null) where TRes: class, IMasterDataModel
    {
      var result = GetMasterDataItemServiceDiscovery<TRes>(route, uid, userId,
        customHeaders, parameters);
      return result;
    }

    protected async Task<TRes> PostData<TReq, TRes>(string route,
      TReq request,
      IList<KeyValuePair<string, string>> parameters = null,
      IDictionary<string, string> customHeaders = null) where TReq : class where TRes : class, IMasterDataModel
    {
      var payload = JsonConvert.SerializeObject(request);

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        return await SendMasterDataItemServiceDiscoveryNoCache<TRes>(route, customHeaders, HttpMethod.Post, parameters);
      }
    }

    protected Task<TRes> DeleteData<TRes>(string route, IList<KeyValuePair<string, string>> parameters = null,
      IDictionary<string, string> customHeaders = null) where TRes : class, IMasterDataModel
    {
      return SendMasterDataItemServiceDiscoveryNoCache<TRes>(route, customHeaders, HttpMethod.Delete, parameters);
    }

    protected async Task<TRes> UpdateData<TReq, TRes>(string route,
      TReq request,
      IList<KeyValuePair<string, string>> parameters = null,
      IDictionary<string, string> customHeaders = null) where TReq : class where TRes : class, IMasterDataModel
    {
      var payload = JsonConvert.SerializeObject(request);

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload)))
      {
        // Need to await this, as we need the stream (if we return the task, the stream is disposed)
        return await SendMasterDataItemServiceDiscoveryNoCache<TRes>(route, customHeaders, HttpMethod.Put, parameters, ms);
      }
    }

  }
}
