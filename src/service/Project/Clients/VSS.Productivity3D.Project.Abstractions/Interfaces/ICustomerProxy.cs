using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Productivity3D.Project.Abstractions.Interfaces
{
  public interface ICustomerProxy : ICacheProxy
  {
    // todoMaverick should users call IAccountClient (e.g. GetMyAccounts())  directly or via ProjectSvc?
    Task<AccountListResponseModel> GetCustomersForMe(string userUid, IDictionary<string, string> customHeaders);

    Task<AccountListResponseModel> GetCustomersForUser(string userUid, IDictionary<string, string> customHeaders);

    Task<AccountResponseModel> GetCustomerForUser(string userUid, string customerUid,
      IDictionary<string, string> customHeaders = null); 
    
    Task<DeviceLicenseResponseModel> GetDeviceLicenses(string customerUid);
  }
}
