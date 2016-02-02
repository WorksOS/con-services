using Newtonsoft.Json;
using System;
using VSP.MasterData.Customer.Data.Interfaces;

namespace VSP.MasterData.Customer.Data.Models
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
