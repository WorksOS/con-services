//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using VSS.Common.Abstractions.Cache.Interfaces;
//using VSS.Common.Abstractions.Configuration;
//using VSS.Common.Abstractions.ServiceDiscovery.Enums;
//using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
//using VSS.MasterData.Models.Models;
//using VSS.MasterData.Models.ResultHandling;
//using VSS.MasterData.Proxies.Interfaces;

//namespace VSS.MasterData.Proxies
//{
//  public class UnifiedProductivityProxyObsolete : BaseServiceDiscoveryProxy, IUnifiedProductivityProxyObsolete
//  {
//    public UnifiedProductivityProxyObsolete(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
//      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
//      dataCache, serviceResolution)
//    {
//    }

//    //unifiedproductivity_public_v1 = https://api.trimble.com/t/trimble.com/vss-unifiedproductivity/1.0/

//    //https://api.trimble.com/t/trimble.com/vss-unifiedproductivity/1.0/composite/projects/{projectUID}/sitewithtargets/asgeofence 

//    public override bool IsInsideAuthBoundary => false;
//    public override ApiService InternalServiceType => ApiService.None;
//    public override string ExternalServiceName => "unifiedproductivity";
//    public override ApiVersion Version => ApiVersion.V1;
//    public override ApiType Type => ApiType.Public;
//    public override string CacheLifeKey => "UNIFIED_PRODUCTIVITY_CACHE_LIFE";

//    /// <summary>
//    /// Gets the list of geofences associated with the project
//    /// </summary>
//    public async Task<List<GeofenceData>> GetAssociatedGeofences(string projectUid, IDictionary<string, string> customHeaders = null)
//    {
//      var route = $"composite/projects/{projectUid}/sitewithtargets/asgeofence";
//      var result = await GetMasterDataItemServiceDiscovery<GeofenceWithTargetsResult>(route, projectUid, null,
//        customHeaders, null);
//      return result.Results?.Select(r => r.Geofence).ToList();
//    } 

//  }
//}
