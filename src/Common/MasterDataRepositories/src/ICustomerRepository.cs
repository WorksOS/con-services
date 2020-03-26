using System;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface ICustomerRepository
  {
    Task<CustomerTccOrg> GetCustomerWithTccOrg(Guid customerUid);
    Task<CustomerTccOrg> GetCustomerWithTccOrg(string tccOrgUid);

    //todoMaverick this repo is only needed temporarily for TBC call to our endpoints
    //    Entries in this db need to be made manually, to link our customerUid (now cws accountTrn) to a tccOrgId
    //Task<Customer> GetAssociatedCustomerbyUserUid(Guid userUid);
    //Task<Customer> GetCustomer(Guid customerUid);
    //Task<CustomerUser> GetAssociatedCustomerbyUserUid_UnitTest(Guid userUid);
    
    // these are for unit tests only
    Task<int> StoreEvent(ICustomerEvent evt);
    Task<Customer> GetCustomer_UnitTest(Guid customerUid);
  }
}
