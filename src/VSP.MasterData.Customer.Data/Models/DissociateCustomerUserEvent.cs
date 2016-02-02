using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSP.MasterData.Customer.Data.Interfaces;

namespace VSP.MasterData.Customer.Data.Models
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
