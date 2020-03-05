using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.DissociateProjectSubscription
{

  public class Modelstate
  {
    [JsonProperty(PropertyName = "dissociateprojectSubscription.SubscriptionUID")]
    public List<string> SubscriptionUID { get; set; }
    [JsonProperty(PropertyName = "dissociateprojectSubscription.ProjectUID")]
    public List<string> ProjectUID { get; set; }
    [JsonProperty(PropertyName = "dissociateprojectSubscription.EffectiveDate")]
    public List<string> EffectiveDate { get; set; }
    [JsonProperty(PropertyName = "dissociateprojectSubscription.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class DissociateProjectSubscriptionServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate Modelstate { get; set; }
  }
}
