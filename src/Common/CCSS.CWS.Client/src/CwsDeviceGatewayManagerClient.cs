using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  public abstract class CwsDeviceGatewayManagerClient : BaseClient
  {
    public override string ExternalServiceName => "cws_devicegateway";
    public override ApiVersion Version => ApiVersion.V2;

    public override string CacheLifeKey => "CWS_DEVICEGATEWAY_CACHE_LIFE"; // we might want to make this much quicker than the default

    protected CwsDeviceGatewayManagerClient(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
     IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
     dataCache, serviceResolution)
    { }
  }
}
