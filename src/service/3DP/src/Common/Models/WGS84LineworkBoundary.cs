using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models.Files;

namespace VSS.Productivity3D.Common.Models
{
  public class WGS84LineworkBoundary
  {
    [JsonProperty(PropertyName = "boundary", Required = Required.Always)]
    public WGSPoint[] Boundary;

    [JsonProperty(PropertyName = "boundaryType", Required = Required.Always)]
    public DXFLineWorkBoundaryType BoundaryType = DXFLineWorkBoundaryType.Unknown;

    [JsonProperty(PropertyName = "boundaryName", Required = Required.Always)]
    public string BoundaryName = "";
  }
}
