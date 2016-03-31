using System;
using Newtonsoft.Json;
using VSS.Customer.Data.Interfaces;

namespace VSS.Customer.Data.Models
{
  public enum CustomerType
  {
    Customer = 0,
    Dealer = 1,
    Operations = 2,
    Corporate = 3
  }

  public class CreateCustomerEvent : ICustomerEvent
  {
    public string CustomerName { get; set; }
    public CustomerType CustomerType { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string BSSID { get; set; }

    //[Required]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DealerNetwork { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string NetworkDealerCode { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string NetworkCustomerCode { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string DealerAccountCode { get; set; }

    // [Required]
    public Guid CustomerUID { get; set; }
    //[Required]
    public DateTime ActionUTC { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime ReceivedUTC { get; set; }
  }
}
