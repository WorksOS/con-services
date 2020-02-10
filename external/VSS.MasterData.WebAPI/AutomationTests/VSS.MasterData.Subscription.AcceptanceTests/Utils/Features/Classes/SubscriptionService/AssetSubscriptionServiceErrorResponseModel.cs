using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Features.Classes.SubscriptionService.AssetSubsciption
{

  public class Modelstate
  {
    [JsonProperty(PropertyName = "assetSubscription.SubscriptionUID")]
    public List<string> SubscriptionUID { get; set; }
    [JsonProperty(PropertyName = "assetSubscription.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "assetSubscription.AssetUID")]
    public List<string> AssetUID { get; set; }
    [JsonProperty(PropertyName = "assetSubscription.DeviceUID")]
    public List<string> DeviceUID { get; set; }
    [JsonProperty(PropertyName = "assetSubscription.StartDate")]
    public List<string> StartDate { get; set; }
    [JsonProperty(PropertyName = "assetSubscription.SubscriptionType")]
    public List<string> SubscriptionType { get; set; }
    [JsonProperty(PropertyName = "assetSubscription.EndDate")]
    public List<string> EndDate { get; set; }
    [JsonProperty(PropertyName = "assetSubscription.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class AssetSubscriptionServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate Modelstate { get; set; }
  }
}
