using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Customer.Data.Interfaces
{
  public interface ICustomerService
  {
    //IEnumerable<Common.Models.Customer> GetAssociatedCustomerbyUserUid(Guid userUID);
    //Common.Models.Customer GetCustomer(Guid customerUID);
    int StoreCustomer(ICustomerEvent evt);
  }
}

