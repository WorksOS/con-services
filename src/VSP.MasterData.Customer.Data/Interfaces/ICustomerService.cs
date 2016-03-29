using System;
using System.Collections.Generic;
using VSS.Customer.Data.Models;

namespace VSS.Customer.Data.Interfaces
{
  public interface ICustomerService
  {
    //void CreateCustomer(CreateCustomerEvent createCustomerEvent);
    //int UpdateCustomer(UpdateCustomerEvent updateCustomerEvent);
    //void DeleteCustomer(DeleteCustomerEvent deleteCustomerEvent);
    //bool AssociateCustomerUser(AssociateCustomerUserEvent associateCustomerUserEvent);
    //void DissociateCustomerUser(DissociateCustomerUserEvent dissociateCustomerUserEvent);
    List<Models.Customer> GetAssociatedCustomerbyUserUid(Guid UserUID);
    Models.Customer GetCustomer(Guid CustomerUID);
    int StoreCustomer(ICustomerEvent evt);
    IEnumerable<Models.Customer> GetCustomers();
  }
}

