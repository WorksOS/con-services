using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface ICustomerProxy : ICacheProxy
  {
    // todoMaverick should users call IAccountClient (e.g. GetMyAccounts())  directly or via ProjectSvc?
    Task<CustomerDataResult> GetCustomersForMe(string userUid, IDictionary<string, string> customHeaders);

    Task<List<CustomerData>> GetCustomersForUser(string userUid, IDictionary<string, string> customHeaders);

    Task<CustomerData> GetCustomerForUser(string userUid, string customerUid,
      IDictionary<string, string> customHeaders = null); 
    
    Task<int> GetDeviceLicenses(string customerUid);
  }
}
