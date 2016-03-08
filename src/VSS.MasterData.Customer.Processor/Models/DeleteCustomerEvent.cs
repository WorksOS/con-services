using Newtonsoft.Json;
using System;
using VSS.MasterData.Customer.Processor.Interfaces;

namespace VSS.MasterData.Customer.Processor.Models
{
    public class DeleteCustomerEvent : ICustomerEvent
    {
        //[Required]
        public Guid CustomerUID { get; set; }
        //[Required]
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ReceivedUTC { get; set; }
    }
}
