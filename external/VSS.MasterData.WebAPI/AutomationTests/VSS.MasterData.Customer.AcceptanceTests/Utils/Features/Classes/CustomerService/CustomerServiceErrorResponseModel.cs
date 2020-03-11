using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerService
{
  public class Modelstate
  {
    [JsonProperty(PropertyName = "customer.CustomerName")]
    public List<string> CustomerName { get; set; }
    [JsonProperty(PropertyName = "customer.CustomerType")]
    public List<string> CustomerType { get; set; }
    [JsonProperty(PropertyName = "customer.BSSID")]
    public List<string> BSSID { get; set; }
    [JsonProperty(PropertyName = "customer.DealerNetwork")]
    public List<string> DealerNetwork { get; set; }
    [JsonProperty(PropertyName = "customer.NetworkDealerCode")]
    public List<string> NetworkDealerCode { get; set; }
    [JsonProperty(PropertyName = "customer.NetworkCustomerCode")]
    public List<string> NetworkCustomerCode { get; set; }
    [JsonProperty(PropertyName = "customer.DealerAccountCode")]
    public List<string> DealerAccountCode { get; set; }
    [JsonProperty(PropertyName = "customer.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "customer.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class CustomerServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate ModelState { get; set; }
  }
}
