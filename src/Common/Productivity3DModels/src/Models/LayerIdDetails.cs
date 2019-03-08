using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.Converters;

namespace VSS.Productivity3D.Models.Models
{
  public class LayerIdDetails
  {
    [JsonConverter(typeof(FormatLongAsStringConverter))]
    public long AssetId { get; set; }
    public long DesignId { get; set; }
    public long LayerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? AssetUid { get; set; }

    public LayerIdDetails(long assetId, long designId, long layerId,
      DateTime startDate, DateTime endDate,
      Guid? assetUid = null
    )
    {
      AssetId = assetId;
      DesignId = designId;
      LayerId = layerId;
      StartDate = startDate;
      EndDate = endDate;
      AssetUid = assetUid;
    }
  }
}
