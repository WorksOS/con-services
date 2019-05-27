using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.Models
{
  public class GeofenceWithTargetsData 
  {
    public GeofenceData Geofence { get; set; }
    //Unified Productivity also returns Target and Backfill data here which we don't care about
  }
}
