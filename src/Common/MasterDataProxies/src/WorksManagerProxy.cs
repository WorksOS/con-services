using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;

namespace VSS.MasterData.Proxies
{
  public class WorksManagerProxy : BaseServiceDiscoveryProxy
  {
    public WorksManagerProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
      IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
      dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => false;
    public override ApiService InternalServiceType => ApiService.None;
    public override string ExternalServiceName => "WorksManager";
    public override ApiVersion Version => ApiVersion.V1;
    public override ApiType Type => ApiType.Public;
    public override string CacheLifeKey => "";


    public async Task<IL2ConnectedSiteMessage> GetMachineStatus() { throw new NotImplementedException(); }


  }
}
