using System;
using Newtonsoft.Json;
using VSS.Customer.Data.Interfaces;

namespace VSS.Customer.Data.Models
{
    public class UpdateCustomerEvent : ICustomerEvent
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CustomerName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BSSID { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerNetwork { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkDealerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NetworkCustomerCode { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DealerAccountCode { get; set; }
        //[Required]
        public Guid CustomerUID { get; set; }
        //[Required]
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ReceivedUTC { get; set; }
    }
}
