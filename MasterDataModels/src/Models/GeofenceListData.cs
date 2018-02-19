using System.Collections.Generic;

namespace VSS.MasterData.Models.Models
{
  public class GeofenceListData : BaseDataResult
  {
    public List<GeofenceData> GeofenceData { get; set; }
  }
}
