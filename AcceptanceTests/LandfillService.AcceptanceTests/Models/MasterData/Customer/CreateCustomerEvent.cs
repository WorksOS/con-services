using System;

namespace LandfillService.AcceptanceTests.Models
{
    public class CreateCustomerEvent
    {
        public string CustomerName { get; set; }
        public CustomerType CustomerType { get; set; }
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