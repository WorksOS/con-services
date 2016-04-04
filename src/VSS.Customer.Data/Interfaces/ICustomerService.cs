using System;
using System.Collections.Generic;
using VSS.Customer.Data.Models;

namespace VSS.Customer.Data.Interfaces
{
  public interface ICustomerService
  {
    Models.Customer GetAssociatedCustomerbyUserUid(Guid userUID);
    Models.Customer GetCustomer(Guid customerUID);
    int StoreCustomer(ICustomerEvent evt);
    IEnumerable<Models.Customer> GetCustomers();
  }
}

