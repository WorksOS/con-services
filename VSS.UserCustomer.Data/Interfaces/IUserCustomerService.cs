using System;
using System.Collections.Generic;

namespace VSS.UserCustomer.Data.Interfaces
{
  public interface IUserCustomerService
  {
    int StoreUserCustomer(IUserCustomerEvent evt);
  }
}
