using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService
{
  public class Subscription
  {
    [JsonProperty(PropertyName = "SubscriptionType")]
    public string SubscriptionType { get; set; }
    [JsonProperty(PropertyName = "StartDate")]
    public string StartDate { get; set; }
    [JsonProperty(PropertyName = "EndDate")]
    public string EndDate { get; set; }
  }

  public class SubscriptionServiceReadResponseModel
  {
    public List<Subscription> Subscriptions { get; set; }
  }
}
