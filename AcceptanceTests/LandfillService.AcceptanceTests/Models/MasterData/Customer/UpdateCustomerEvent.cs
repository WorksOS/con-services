using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandfillService.AcceptanceTests.Models.MasterData.Interfaces;

namespace LandfillService.AcceptanceTests.Models.MasterData.Customer
{
    public class UpdateCustomerEvent : ICustomerEvent
    {
        public string CustomerName { get; set; }
        public string BSSID { get; set; }
        public string DealerNetwork { get; set; }
        public string NetworkDealerCode { get; set; }
        public string NetworkCustomerCode { get; set; }
        public string DealerAccountCode { get; set; }
        public Guid CustomerUID { get; set; }
        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
