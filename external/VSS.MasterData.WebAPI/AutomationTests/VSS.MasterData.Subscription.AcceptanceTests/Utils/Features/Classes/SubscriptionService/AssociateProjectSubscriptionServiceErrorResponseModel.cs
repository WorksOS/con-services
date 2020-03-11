using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.AssociateProjectSubscription
{

  public class Modelstate
  {
    [JsonProperty(PropertyName = "associateprojectSubscription.SubscriptionUID")]
    public List<string> SubscriptionUID { get; set; }
    [JsonProperty(PropertyName = "associateprojectSubscription.ProjectUID")]
    public List<string> ProjectUID { get; set; }
    [JsonProperty(PropertyName = "associateprojectSubscription.EffectiveDate")]
    public List<string> EffectiveDate { get; set; }
    [JsonProperty(PropertyName = "associateprojectSubscription.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class AssociateProjectSubscriptionServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate Modelstate { get; set; }
  }
}
