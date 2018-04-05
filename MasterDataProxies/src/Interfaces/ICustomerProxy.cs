using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ICustomerProxy : ICacheProxy
  {
    Task<CustomerDataResult> GetCustomersForMe(string userUid, IDictionary<string, string> customHeaders);

    Task<List<CustomerData>> GetCustomersForUser(string userUid, IDictionary <string, string> customHeaders);

    Task<CustomerData> GetCustomerForUser(string userUid, string customerUid,
      IDictionary<string, string> customHeaders = null);
  }
}
