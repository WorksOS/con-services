using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class MockProxy : BaseServiceDiscoveryProxy
  {
    private bool _isInsideAuthBoundary;
    private ApiService internalServiceType;
    private string externalServiceName;
    private ApiVersion version;
    private ApiType type;

    public MockProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public void SetParameters(bool isInsideAuthBoundary, ApiService serviceType, string externalServiceName, ApiVersion version, ApiType type)
    {
      this._isInsideAuthBoundary = isInsideAuthBoundary;
      this.internalServiceType = serviceType;
      this.externalServiceName = externalServiceName;
      this.version = version;
      this.type = type;

    }

    public override bool IsInsideAuthBoundary => _isInsideAuthBoundary;
    public override ApiService InternalServiceType => internalServiceType;
    public override string ExternalServiceName => externalServiceName;
    public override ApiVersion Version => version;
    public override ApiType Type => type;
    public override string CacheLifeKey => "NO_CACHE";

    public Task<ContractExecutionResult> DoACall(string route, IDictionary<string, string> customerHeaders)
    {
      return MasterDataItemServiceDiscoveryNoCache<ContractExecutionResult>(route, customerHeaders, HttpMethod.Get);
    }
  }
}