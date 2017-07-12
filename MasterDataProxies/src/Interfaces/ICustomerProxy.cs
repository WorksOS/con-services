using MasterDataModels.ResultHandling;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface ICustomerProxy
  {
    Task<CustomerDataResult> GetCustomersForMe(string userUid, IDictionary < string, string> customHeaders);
  }
}
