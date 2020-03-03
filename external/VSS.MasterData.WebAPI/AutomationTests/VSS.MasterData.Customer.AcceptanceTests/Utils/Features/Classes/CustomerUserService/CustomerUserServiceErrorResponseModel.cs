using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.CustomerUserService
{
  public class Modelstate
  {
    [JsonProperty(PropertyName = "customerUser.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "customerUser.UserUID")]
    public List<string> UserUID { get; set; }
    [JsonProperty(PropertyName = "customerUser.RelationType")]
    public List<string> RelationType { get; set; }
    [JsonProperty(PropertyName = "customerUser.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class CustomerUserServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate ModelState { get; set; }
  }
}
