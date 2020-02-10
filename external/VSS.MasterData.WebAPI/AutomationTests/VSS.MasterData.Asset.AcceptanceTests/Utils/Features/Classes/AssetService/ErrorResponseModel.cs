using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService
{
  public class Modelstate
  {
    [JsonProperty(PropertyName = "asset.AssetUID")]
    public List<string> AssetUID { get; set; }
    [JsonProperty(PropertyName = "asset.SerialNumber")]
    public List<string> SerialNumber { get; set; }
    [JsonProperty(PropertyName = "asset.MakeCode")]
    public List<string> MakeCode { get; set; }
    [JsonProperty(PropertyName = "asset.ActionUTC")]
    public List<string> ActionUTC { get; set; }
    [JsonProperty(PropertyName = "asset.IconKey")]
    public List<string> IconKey { get; set; }
    [JsonProperty(PropertyName = "asset.ModelYear")]
    public List<string> ModelYear { get; set; }
  }

  public class ErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate ModelState { get; set; }
  }

}

