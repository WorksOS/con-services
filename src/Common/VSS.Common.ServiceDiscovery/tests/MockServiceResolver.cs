using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;

namespace VSS.Common.ServiceDiscovery.UnitTests
{
  internal class MockServiceResolver : IServiceResolver
  {
    public MockServiceResolver(ServiceResultType resultType, int priority)
    {
      ServiceMap = new Dictionary<string, string>();
      ServiceType = resultType;
      Priority = priority;
    }

    public Dictionary<string, string> ServiceMap { get; }

    public Task<string> ResolveService(string serviceName)
    {
      return Task.FromResult(ServiceMap.ContainsKey(serviceName) 
        ? ServiceMap[serviceName] 
        : null);
    }

    public ServiceResultType ServiceType { get; private set; }
    public int Priority { get; private set; }
  }
}