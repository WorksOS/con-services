using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSP.MasterData.Customer.Data.Interfaces;

namespace VSP.MasterData.Customer.Data.Models
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
