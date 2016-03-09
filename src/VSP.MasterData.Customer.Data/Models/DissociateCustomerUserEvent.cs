using System;
using Newtonsoft.Json;
using VSS.Customer.Data.Interfaces;

namespace VSS.Customer.Data.Models
{
    public class DissociateCustomerUserEvent : ICustomerUserEvent
    {
        //[Required]
        public Guid CustomerUID { get; set; }
        //[Required]
        public Guid UserUID { get; set; }
        //[Required]
        public DateTime ActionUTC { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime ReceivedUTC { get; set; }
    }
}
