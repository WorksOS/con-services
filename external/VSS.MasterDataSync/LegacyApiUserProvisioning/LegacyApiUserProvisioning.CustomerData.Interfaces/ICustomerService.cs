using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyApiUserProvisioning.CustomerData.Interfaces
{
    public interface ICustomerService
    {
        IEnumerable<ICustomer> GetInActiveCustomers(string filter, int maxResults = 20);
        IEnumerable<ICustomer> GetActiveCustomers(string filter, int maxResults = 20);
        IEnumerable<ICustomer> GetActiveInactiveCustomers(string filter, int maxResults = 20);
        IEnumerable<ICustomer> GetCustomersByBssid(string trim, bool active = true, bool exactMatch = true, int maxResults = 20);
    }
}
