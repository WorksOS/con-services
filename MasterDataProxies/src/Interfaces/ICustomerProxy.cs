using System.Collections.Generic;
using System.Threading.Tasks;
using MasterDataProxies.ResultHandling;

namespace MasterDataProxies.Interfaces
{
  public interface ICustomerProxy
  {
    Task<ContractExecutionResult> GetCustomersForMe(IDictionary < string, string> customHeaders);
  }
}
