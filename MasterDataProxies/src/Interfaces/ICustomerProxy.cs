using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterDataProxies.Interfaces
{
  public interface ICustomerProxy
  {
    Task<CustomerDataResult> GetCustomersForMe(string userUid, IDictionary < string, string> customHeaders);
  }
}
