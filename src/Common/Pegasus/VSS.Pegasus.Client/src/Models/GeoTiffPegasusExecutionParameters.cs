using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class GeoTiffPegasusExecutionParameters : PegasusExecutionParameters
  {
    [JsonProperty(PropertyName = "dataocean_uuids", Required = Required.Always)]
    public Guid GeoTiffFileId { get; set; }
    [JsonProperty(PropertyName = "tile_export_format", Required = Required.Default)]
    public string TileExportFormat { get; set; }
    [JsonProperty(PropertyName = "tile_output_format", Required = Required.Default)]
    public string TileOutputFormat { get; set; }
    [JsonProperty(PropertyName = "tile_crs", Required = Required.Default)]
    public string TileCrs { get; set; }
  }
}
