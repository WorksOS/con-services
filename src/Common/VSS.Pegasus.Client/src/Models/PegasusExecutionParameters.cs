using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecutionParameters
  {
    [JsonProperty(PropertyName = "dc_file_id", Required = Required.Always)]
    public Guid DcFileId { get; set; }
    [JsonProperty(PropertyName = "dxf_file_id", Required = Required.Always)]
    public Guid DxfFileId { get; set; }
    [JsonProperty(PropertyName = "parent_id", Required = Required.Default)]
    public Guid? ParentId { get; set; }
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; set; }
    [JsonProperty(PropertyName = "max_zoom", Required = Required.Default)]
    public int MaxZoom { get; set; }
    [JsonProperty(PropertyName = "tile_type", Required = Required.Default)]
    public string TileType { get; set; }
    [JsonProperty(PropertyName = "tile_order", Required = Required.Default)]
    public string TileOrder { get; set; }
    [JsonProperty(PropertyName = "multifile", Required = Required.Default)]
    public bool MultiFile { get; set; }
    [JsonProperty(PropertyName = "public", Required = Required.Default)]
    public bool Public { get; set; }
    [JsonProperty(PropertyName = "angular_unit", Required = Required.Default)]
    public string AngularUnit { get; set; }
    [JsonProperty(PropertyName = "plane_unit", Required = Required.Default)]
    public string PlaneUnit { get; set; }
    [JsonProperty(PropertyName = "vertical_unit", Required = Required.Default)]
    public string VerticalUnit { get; set; }


  }
}
