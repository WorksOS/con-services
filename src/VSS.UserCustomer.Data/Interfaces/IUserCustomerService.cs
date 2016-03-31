using System;
using System.Collections.Generic;

namespace VSS.UserCustomer.Data.Interfaces
{
  public interface IUserCustomerService
  {
    Models.UserCustomer GetUserCustomer(Int32 UserCustomerID);
    int StoreUserCustomer(IUserCustomerEvent evt);
    IEnumerable<Models.UserCustomer> GetUserCustomers();
  }
}
