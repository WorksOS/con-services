using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSP.MasterData.Customer.WebAPI.Models
{
    public class AssociatedCustomer
    {
        [JsonProperty("uid")]
        public Guid CustomerUID { get; set; }

        [JsonProperty("name")]
        public string CustomerName { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CustomerType CustomerType { get; set; }
    }
}