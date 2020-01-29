using System;
using System.Collections.Generic;
using System.Linq;
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
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Notification.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Proxy
{
  /// <summary>
  /// Used to execute the Base ExecuteGenericV2Request() method for 3dp tag file controller in VSS
  /// </summary>
  public class Productivity3dV2ProxyVSS : Productivity3dV2Proxy, IProductivity3dV2ProxyVSS
  {
    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Productivity3DVSS;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V2;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PRODUCTIVITY3D_VSS_CACHE_LIFE"; // not used

    public Productivity3dV2ProxyVSS(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

  }
}
