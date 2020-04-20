using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  public abstract class CwsProfileSettingsManagerClient : BaseClient
  {  
    public override string ExternalServiceName => "cws_profilesettings";
    public override ApiVersion Version => ApiVersion.V1;

    protected CwsProfileSettingsManagerClient(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger,
     IDataCache dataCache, IServiceResolution serviceResolution) : base(webRequest, configurationStore, logger,
     dataCache, serviceResolution)
    {
    }
  }
}
