using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.MasterData.AcceptanceTests.Utils.Features.Classes.GeofenceService
{
  public class Modelstate
  {
    [JsonProperty(PropertyName = "geofence.GeofenceName")]
    public List<string> GeofenceName { get; set; }
    [JsonProperty(PropertyName = "geofence.Description")]
    public List<string> Description { get; set; }
    [JsonProperty(PropertyName = "geofence.GeofenceType")]
    public List<string> GeofenceType { get; set; }
    [JsonProperty(PropertyName = "geofence.GeometryWKT")]
    public List<string> GeometryWKT { get; set; }
    [JsonProperty(PropertyName = "geofence.FillColor")]
    public List<string> FillColor { get; set; }
    [JsonProperty(PropertyName = "geofence.IsTransparent")]
    public List<string> IsTransparent { get; set; }
    [JsonProperty(PropertyName = "geofence.CustomerUID")]
    public List<string> CustomerUID { get; set; }
    [JsonProperty(PropertyName = "geofence.UserUID")]
    public List<string> UserUID { get; set; }
    [JsonProperty(PropertyName = "geofence.GeofenceUID")]
    public List<string> GeofenceUID { get; set; }
    [JsonProperty(PropertyName = "geofence.ActionUTC")]
    public List<string> ActionUTC { get; set; }
  }

  public class GeofenceServiceErrorResponseModel
  {
    public string Message { get; set; }
    public Modelstate ModelState { get; set; }
  }
}
