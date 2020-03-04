using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.CustomerSubscription
{

  public class Modelstate
  {
    [JsonProperty(PropertyName = "customerSubscription.SubscriptionUID")]
    public List<string> SubscriptionUID { get; set; }
    [JsonProperty(PropertyName = "customerSubscription.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "customerSubscription.StartDate")]
    public List<string> StartDate { get; set; }
    [JsonProperty(PropertyName = "customerSubscription.SubscriptionType")]
    public List<string> SubscriptionType { get; set; }
    [JsonProperty(PropertyName = "customerSubscription.EndDate")]
    public List<string> EndDate { get; set; }
    [JsonProperty(PropertyName = "customerSubscription.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class CustomerSubscriptionServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate Modelstate { get; set; }
  }
}
