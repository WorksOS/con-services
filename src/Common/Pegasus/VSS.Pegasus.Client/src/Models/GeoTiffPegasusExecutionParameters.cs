using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class GeoTiffPegasusExecutionParameters : PegasusExecutionParameters
  {
    [JsonProperty(PropertyName = "dataocean_uuids", Required = Required.Always)]
    public Guid GeoTiffFileId { get; set; }
    //TODO: other params specific to geotiff
    /*
     *{
    "execution": {
        "procedure_id": "f61c965b-0828-40b6-8980-26c7ee164566",
        "parameters": {
            "parent_id": "b431ef42-3e67-4196-99f9-153864098f45",
            "multifile": "true",
            "public": "false",
            "name": "Kettlewell_Drive_01_Apr_2019_GeoTIFF_WGS_84_Tiles$_3857",
            "dataocean_uuids": "bcafb349-0104-40ae-8cb8-05da50d3f715",
            "tile_export_format": "xyz",
            "tile_output_format": "PNGRASTER",
            "tile_order": "YX",
            "tile_crs": "EPSG:3857"
        }
    }
}
     */
  }
}
