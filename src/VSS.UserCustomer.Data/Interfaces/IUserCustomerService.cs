using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.UserCustomer.Data.Interfaces
{
  public interface IUserCustomerService
  {
    int StoreUserCustomer(ICustomerUserEvent evt);
  }
}
