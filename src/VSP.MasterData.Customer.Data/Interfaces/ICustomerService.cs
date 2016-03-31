using System;
using System.Collections.Generic;
using VSS.Customer.Data.Models;

namespace VSS.Customer.Data.Interfaces
{
  public interface ICustomerService
  {
    List<Models.Customer> GetAssociatedCustomerbyUserUid(Guid UserUID);
    Models.Customer GetCustomer(Guid CustomerUID);
    int StoreCustomer(ICustomerEvent evt);
    IEnumerable<Models.Customer> GetCustomers();
  }
}

