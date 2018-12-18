using System;
using System.Threading.Tasks;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public interface ICustomerRepository
  {

    Task<Customer> GetAssociatedCustomerbyUserUid(Guid userUid);
    Task<Customer> GetCustomer(Guid customerUid);
    Task<CustomerTccOrg> GetCustomerWithTccOrg(Guid customerUid);
    Task<CustomerTccOrg> GetCustomerWithTccOrg(string tccOrgUid);
    Task<Customer> GetCustomer_UnitTest(Guid customerUid);
    Task<CustomerUser> GetAssociatedCustomerbyUserUid_UnitTest(Guid userUid);

    Task<int> StoreEvent(ICustomerEvent filterEvent);
  }
}