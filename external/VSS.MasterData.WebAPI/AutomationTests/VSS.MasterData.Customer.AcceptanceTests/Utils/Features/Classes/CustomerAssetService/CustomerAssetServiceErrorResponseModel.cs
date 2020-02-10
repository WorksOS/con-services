using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerAssetService
{
  public class Modelstate
  {
    [JsonProperty(PropertyName = "customerasset.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "customerasset.AssetUID")]
    public List<string> AssetUID { get; set; }
    [JsonProperty(PropertyName = "customerasset.RelationType")]
    public List<string> RelationType { get; set; }
    [JsonProperty(PropertyName = "customerasset.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class CustomerAssetServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate ModelState { get; set; }
  }
}
