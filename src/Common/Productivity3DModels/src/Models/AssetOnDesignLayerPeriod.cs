using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.Converters;

namespace VSS.Productivity3D.Models.Models
{
  public class AssetOnDesignLayerPeriod
  {
    [JsonConverter(typeof(FormatLongAsStringConverter))]
    public long AssetId { get; set; }
    [JsonProperty(PropertyName = "DesignId")]
    public long OnMachineDesignId { get; set; }
    public long LayerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? AssetUid { get; set; }
    [JsonProperty(PropertyName = "DesignName")]
    public string OnMachineDesignName { get; set; }

    public AssetOnDesignLayerPeriod(long assetId, long onMachineDesignId, long layerId,
      DateTime startDate, DateTime endDate,
      Guid? assetUid = null, string onMachineDesignName = null
    )
    {
      AssetId = assetId;
      OnMachineDesignId = onMachineDesignId;
      LayerId = layerId;
      StartDate = startDate;
      EndDate = endDate;
      AssetUid = assetUid;
      OnMachineDesignName = onMachineDesignName;
    }
  }
}
