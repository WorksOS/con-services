using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.SubscriptionService
{

  public class Modelstate
  {
    [JsonProperty(PropertyName = "subscription.SubscriptionUID")]
    public List<string> SubscriptionUID { get; set; }
    [JsonProperty(PropertyName = "subscription.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "subscription.AssetUID")]
    public List<string> AssetUID { get; set; }
    [JsonProperty(PropertyName = "subscription.SubscriptionTypeID")]
    public List<string> StartDate { get; set; }
    [JsonProperty(PropertyName = "subscription.StartDate")]
    public List<string> SubscriptionTypeID { get; set; }
    [JsonProperty(PropertyName = "subscription.EndDate")]
    public List<string> EndDate { get; set; }
    [JsonProperty(PropertyName = "subscription.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class SubscriptionServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate ModelState { get; set; }
  }
}
