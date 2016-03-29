using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Customer.Data.Interfaces
{
  public interface IUserCustomerService
  {
    Models.UserCustomer GetUserCustomer(Int32 UserCustomerID);
    int StoreUserCustomer(IUserCustomerEvent evt);
    IEnumerable<Models.UserCustomer> GetUserCustomers();
  }
}
