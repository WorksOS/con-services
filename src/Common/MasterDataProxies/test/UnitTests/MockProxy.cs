using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    private ApiService _internalServiceType;
    private string _externalServiceName;
    private ApiVersion _version;
    private ApiType _type;

    public MockProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public void SetParameters(bool isInsideAuthBoundary, ApiService serviceType, string externalServiceName, ApiVersion version, ApiType type)
    {
      _isInsideAuthBoundary = isInsideAuthBoundary;
      _internalServiceType = serviceType;
      _externalServiceName = externalServiceName;
      _version = version;
      _type = type;
    }

    public override bool IsInsideAuthBoundary => _isInsideAuthBoundary;
    public override ApiService InternalServiceType => _internalServiceType;
    public override string ExternalServiceName => _externalServiceName;
    public override ApiVersion Version => _version;
    public override ApiType Type => _type;
    public override string CacheLifeKey => "NO_CACHE";

    public Task<ContractExecutionResult> DoACall(string route, IHeaderDictionary customerHeaders)
    {
      return MasterDataItemServiceDiscoveryNoCache<ContractExecutionResult>(route, customerHeaders, HttpMethod.Get);
    }
  }
}
