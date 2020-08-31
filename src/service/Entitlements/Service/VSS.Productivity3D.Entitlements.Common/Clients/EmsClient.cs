using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Entitlements.Common.Clients
{
  /// <summary>
  /// Client for TPaaS EMS (entitlement management system) 
  /// </summary>
  public class EmsClient : BaseServiceDiscoveryProxy
  {
    public override string ExternalServiceName => "entitlements";
    public override ApiVersion Version => ApiVersion.V1;
    public override bool IsInsideAuthBoundary => false;
    public override ApiService InternalServiceType => ApiService.None;
    public override ApiType Type => ApiType.Public;
    public override string CacheLifeKey => "ENTITLEMENTS_CACHE_LIFE";

    protected EmsClient(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    { }

    /// <summary>
    /// Determine if the user is entitled to use the specified feature
    /// </summary>
    protected Task<HttpStatusCode> GetEntitlements(Guid userUid, Guid? customerUid, string wosSku, string wosFeature,  IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetEntitlements)}: userUid={userUid}, customerUid={customerUid}, wosSku={wosSku}, wosFeature={wosFeature}");

      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("fields", "full"),
        new KeyValuePair<string, string>("featurePrefixes", wosFeature),
        new KeyValuePair<string, string>("skus", wosSku),
        new KeyValuePair<string, string>("states", "ACTIVATED"),
      };
      if (customerUid.HasValue)
        queryParams.Add(new KeyValuePair<string, string>("customerid", customerUid.Value.ToString()));
      return SendMasterDataItemServiceDiscoveryNoCache($"entitlements/members/{userUid}/activations", customHeaders, HttpMethod.Get, queryParams);
    }
  }
}
