using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyApiUserProvisioning.CustomerData.Interfaces;

namespace LegacyApiUserProvisioning.CustomerData
{
    public class Customer: ICustomer
    {
        public Guid CustomerUID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string NetworkCustomerCode { get; set; }
        public string NetworkDealerCode { get; set; }
        public string BSSID { get; set; }
     }
}
