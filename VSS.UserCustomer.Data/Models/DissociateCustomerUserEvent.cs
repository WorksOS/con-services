using System;
using Newtonsoft.Json;
using VSS.UserCustomer.Data.Interfaces;

namespace VSS.UserCustomer.Data.Models
{
    public class DissociateCustomerUserEvent : IUserCustomerEvent
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
