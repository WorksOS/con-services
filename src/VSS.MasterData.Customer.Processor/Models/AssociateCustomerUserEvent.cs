using Newtonsoft.Json;
using System;
using VSS.MasterData.Customer.Processor.Interfaces;

namespace VSS.MasterData.Customer.Processor.Models
{
    public class AssociateCustomerUserEvent : ICustomerUserEvent
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
