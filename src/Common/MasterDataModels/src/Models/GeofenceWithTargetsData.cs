using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.Models
{
  public class GeofenceWithTargetsData : IMasterDataModel
  {
    public GeofenceData Geofence { get; set; }
    //Unified Productivity also returns Target and Backfill data here which we don't care about

    public List<string> GetIdentifiers() => Geofence?.GetIdentifiers() ?? new List<string>();
  }
}
