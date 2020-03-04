using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.ProjectSubscription
{

  public class Modelstate
  {
    [JsonProperty(PropertyName = "projectSubscription.SubscriptionUID")]
    public List<string> SubscriptionUID { get; set; }
    [JsonProperty(PropertyName = "projectSubscription.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "projectSubscription.StartDate")]
    public List<string> StartDate { get; set; }
    [JsonProperty(PropertyName = "projectSubscription.SubscriptionType")]
    public List<string> SubscriptionType { get; set; }
    [JsonProperty(PropertyName = "projectSubscription.EndDate")]
    public List<string> EndDate { get; set; }
    [JsonProperty(PropertyName = "projectSubscription.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class ProjectSubscriptionServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate Modelstate { get; set; }
  }
}
