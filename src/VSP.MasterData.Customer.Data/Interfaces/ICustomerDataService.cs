

using System;
using System.Collections.Generic;
using VSP.MasterData.Customer.Data.Models;
namespace VSP.MasterData.Customer.Data
{
  public interface ICustomerDataService
  {
    void CreateCustomer(CreateCustomer createCustomerEvent);
    int UpdateCustomer(UpdateCustomer updateCustomerEvent);
    void DeleteCustomer(DeleteCustomer deleteCustomerEvent);
    bool AssociateCustomerUser(AssociateCustomerUser associateCustomerUserEvent);
    void DissociateCustomerUser(DissociateCustomerUser dissociateCustomerUserEvent);
    List<Models.Customer> GetAssociatedCustomerbyUserUid(Guid UserUID);
    Models.Customer GetCustomer(Guid CustomerUID);
  }
}

