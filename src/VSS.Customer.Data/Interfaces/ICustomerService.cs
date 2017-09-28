using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Customer.Data.Interfaces
{
  public interface ICustomerService
  {
    IEnumerable<Models.Customer> GetAssociatedCustomerbyUserUid(Guid userUID);
    Models.Customer GetCustomer(Guid customerUID);
    int StoreCustomer(ICustomerEvent evt);
  }
}

